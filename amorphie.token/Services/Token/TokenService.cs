

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using amorphie.token.core.Dtos;
using amorphie.token.core.Models.Consent;
using amorphie.token.core.Models.InternetBanking;
using amorphie.token.core.Models.Profile;
using amorphie.token.core.Models.Role;
using amorphie.token.data;
using amorphie.token.Services.ClaimHandler;
using amorphie.token.Services.Consent;
using amorphie.token.Services.InternetBanking;
using amorphie.token.Services.Profile;
using amorphie.token.Services.Role;
using amorphie.token.Services.TransactionHandler;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace amorphie.token.Services.Token;

public class TokenService(ILogger<AuthorizationService> logger, IConfiguration configuration, IClientService clientService, IClaimHandlerService claimService,
ITransactionService transactionService, CollectionUsers collectionUsers, IRoleService roleService, IbDatabaseContextMordor ibContextMordor, IConsentService consentService, IUserService userService, DaprClient daprClient, DatabaseContext databaseContext, IbSecurityDatabaseContext securityContext
    , IInternetBankingUserService internetBankingUserService, IProfileService profileService, IbDatabaseContext ibContext) : ServiceBase(logger, configuration), ITokenService
{
    private readonly IClientService _clientService = clientService;
    private readonly IUserService _userService = userService;
    private readonly DaprClient _daprClient = daprClient;
    private readonly DatabaseContext _databaseContext = databaseContext;
    private readonly ITransactionService _transactionService = transactionService;
    private readonly IClaimHandlerService _claimService = claimService;
    private readonly IConsentService _consentService = consentService;
    private IInternetBankingUserService? _internetBankingUserService = internetBankingUserService;
    private IbDatabaseContext _ibContext = ibContext;
    private IbDatabaseContextMordor _ibContextMordor = ibContextMordor;
    private IbSecurityDatabaseContext _ibSecurityContext = securityContext;
    private IProfileService? _profileService = profileService;
    private IRoleService? _roleService = roleService;
    private CollectionUsers _collectionUsers = collectionUsers;


    private TokenInfoDetail _tokenInfoDetail = new();
    private GenerateTokenRequest? _tokenRequest;
    private ClientResponse? _client;
    private ConsentResponse? _consent;
    private LoginResponse? _user;
    private SimpleProfileResponse? _profile;
    private ConsentDto? _selectedConsent;
    private RoleDto? _role;
    private RoleDefinitionDto? _roleDefinition;
    private core.Models.Collection.User? _collectionUser;
    private TokenInfo? _refreshTokenInfo;
    private string? _deviceId;
    
    private DateTime _currentDate = DateTime.Now; 
    public string DeviceId { set => _deviceId = value;}
    
    private async Task PersistTokenInfo()
    {
        await WriteToDb();
        await WriteToCache();
    }

    private async Task WriteToDb()
    {
        foreach (var tokenInfo in _tokenInfoDetail.TokenList)
        {
            if (tokenInfo != null)
                _databaseContext.Tokens.Add(tokenInfo);
        }
        await _databaseContext.SaveChangesAsync();
    }

    private async Task WriteToCache()
    {
        var accessTokenInfo = _tokenInfoDetail.TokenList.FirstOrDefault(t => t.TokenType == TokenType.AccessToken);
        var refreshTokenInfo = _tokenInfoDetail.TokenList.FirstOrDefault(t => t.TokenType == TokenType.RefreshToken);

        if (accessTokenInfo != null)
        {
            await _daprClient.SaveStateAsync(Configuration["DAPR_STATE_STORE_NAME"], accessTokenInfo!.Id.ToString(), accessTokenInfo, metadata: new Dictionary<string, string> { { "ttlInSeconds", _tokenInfoDetail.AccessTokenDuration.ToString() } });
        }
        if (refreshTokenInfo != null)
        {
            await _daprClient.SaveStateAsync(Configuration["DAPR_STATE_STORE_NAME"], refreshTokenInfo!.Id.ToString(), refreshTokenInfo, metadata: new Dictionary<string, string> { { "ttlInSeconds", _tokenInfoDetail.RefreshTokenDuration.ToString() } });
        }
    }

    private async Task<string> CreateIdToken()
    {
        int iat = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        var claims = new List<Claim>()
        {
            new Claim("iat", iat.ToString(), ClaimValueTypes.Integer),
        };

        var identityInfo = _client!.tokens!.FirstOrDefault(t => t.type == 2);
        if (identityInfo == null)
            return string.Empty;

        if (identityInfo.claims != null && identityInfo.claims.Count() > 0)
        {
            if(_tokenRequest.GrantType.Equals("client_credentials"))
            {
                identityInfo.claims = identityInfo.claims.Where(c => !IsUserBasedClaim(c)).ToList();
            }
            var populatedClaims = await _claimService.PopulateClaims(identityInfo.claims, _user, _profile, collectionUser : _collectionUser);
            
            claims.AddRange(populatedClaims);

        }
        if(_user is {})
        {
            claims.Add(new Claim("sub", _user.Reference.ToString()));
        }

        int idDuration = 0;
        try
        {
            idDuration = TimeHelper.ConvertStrDurationToSeconds(identityInfo.duration!);
        }
        catch (FormatException ex)
        {
            Logger.LogError(ex.Message);
            return string.Empty;
        }

        RSACryptoServiceProvider provider1 = new RSACryptoServiceProvider();

        provider1.FromXmlString(_client.PrivateKey!);

        RsaSecurityKey rsaSecurityKey1 = new RsaSecurityKey(provider1);
        var signinCredentials = new SigningCredentials(rsaSecurityKey1, SecurityAlgorithms.RsaSha256);

        var idToken = JwtHelper.GenerateJwt("BurganIam", _client.returnuri, claims,
        expires: DateTime.UtcNow.AddSeconds(idDuration), signingCredentials:signinCredentials);

        _tokenInfoDetail!.IdTokenId = Guid.NewGuid();
        _tokenInfoDetail!.TokenList.Add(JwtHelper.CreateTokenInfo(TokenType.IdToken, _tokenInfoDetail.IdTokenId, _client.id!, DateTime.UtcNow.AddSeconds(idDuration), true, _user!.Reference
        , new List<string>(), _user!.Id, _tokenInfoDetail.AccessTokenId, null, _deviceId));

        return idToken;
    }

    private async Task<string> CreateAccessToken()
    {
        _tokenInfoDetail.AccessTokenId = Guid.NewGuid();

        var tokenClaims = new List<Claim>();
        var excludedScopes = new string[] { "openId", "profile" };
        foreach (var scope in _tokenRequest!.Scopes!.ToArray().Except(excludedScopes))
            tokenClaims.Add(new Claim("scope", scope));

        if (_tokenRequest.GrantType == "client_credentials")
        {
            tokenClaims.Add(new Claim("scope", "client_credentials"));
        }

        if (_tokenRequest.GrantType == "device")
        {
            tokenClaims.Add(new Claim("scope", "device"));
        }

        var accessInfo = _client!.tokens!.FirstOrDefault(t => t.type == 0);
        if (accessInfo == null)
            return string.Empty;

        int accessDuration = 0;
        try
        {
            accessDuration = TimeHelper.ConvertStrDurationToSeconds(accessInfo.duration!);
            if(_consent is not null)
            {
                var consentData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(_consent!.additionalData!);
                if(_consent.consentType!.Equals("OB_Account"))
                {
                    if(_tokenRequest.GrantType.Equals("refresh_token"))
                    {
                        var DateDiffAsSeconds = Convert.ToInt32((_refreshTokenInfo.ExpiredAt.ToLocalTime() - _currentDate).TotalSeconds);
                        accessDuration = DateDiffAsSeconds > 30 * 24 * 60 * 60 ? (30 * 24 * 60 * 60) : DateDiffAsSeconds; 
                    
                    }
                    else
                    {
                        DateTime lastAccessDate = DateTime.Parse(consentData!.hspBlg.iznBlg.erisimIzniSonTrh.ToString());
                        var DateDiffAsSeconds = Convert.ToInt32((lastAccessDate.AddMilliseconds(-1) - _currentDate).TotalSeconds);
                        accessDuration = DateDiffAsSeconds > 30 * 24 * 60 * 60 ? (30 * 24 * 60 * 60) : DateDiffAsSeconds; 
                    }
                }
                if(_consent.consentType.Equals("OB_Payment"))
                {
                    accessDuration = 5 * 60; // 5 min
                }
            }
            _tokenInfoDetail.AccessTokenDuration = accessDuration;
        }
        catch (FormatException ex)
        {
            Logger.LogError(ex.Message);
            return string.Empty;
        }

        if (accessInfo.claims != null && accessInfo.claims.Count() > 0)
        {
            if(_tokenRequest!.GrantType!.Equals("client_credentials"))
            {
                accessInfo.claims = accessInfo.claims.Where(c => !IsUserBasedClaim(c)).ToList();
            }
            var populatedClaims = await _claimService.PopulateClaims(accessInfo.claims, _user, _profile, _consent, collectionUser : _collectionUser);
            tokenClaims.AddRange(populatedClaims);

            tokenClaims.Add(new Claim("client_id", _client.code ?? _client.id));
        
            
        }

        if(_user?.Reference == "99999999998")
        {
            tokenClaims.Add(new Claim("role", "Viewer"));
        }
        else
        {
            try
            {
                if(_roleDefinition is {})
                {
                    tokenClaims.Add(new Claim("role", _roleDefinition.Description));
                }
                else
                {
                    if(_transactionService.RoleKey == 10 || _transactionService.RoleKey == 20)
                    {
                        if(_transactionService.RoleKey == 10)
                            tokenClaims.Add(new Claim("role", "FullAuthorized"));
                        else
                            tokenClaims.Add(new Claim("role", "Viewer"));
                    }
                    else
                    {
                            tokenClaims.Add(new Claim("role", "Viewer"));
                    }
                }
            }
            catch (Exception)
            {
                tokenClaims.Add(new Claim("role", "Viewer"));
            }
        }

        tokenClaims.Add(new Claim("jti", _tokenInfoDetail.AccessTokenId.ToString()));
        
        if (_user != null)
            tokenClaims.Add(new Claim("userId", _user!.Id.ToString()));
        else
            tokenClaims.Add(new Claim("clientAuthorized", "1"));

        tokenClaims.Add(new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()));

        if (accessInfo.privateClaims != null && accessInfo.privateClaims.Count() > 0)
        {
            var populatedPrivateClaims = await _claimService.PopulatePrivateClaims(accessInfo.privateClaims, _user, _profile, _consent, collectionUser : _collectionUser);
            var dictFromPrivateClaims = populatedPrivateClaims.Where(c => c is not null).ToDictionary(c => c!.Value.Key, c => c!.Value.Value);
            await _daprClient.SaveStateAsync(Configuration["DAPR_STATE_STORE_NAME"], $"{_tokenInfoDetail!.AccessTokenId.ToString()}_privateClaims", dictFromPrivateClaims, metadata: new Dictionary<string, string> { { "ttlInSeconds", (_tokenInfoDetail.AccessTokenDuration+60).ToString() } });
        }

        RSACryptoServiceProvider provider1 = new RSACryptoServiceProvider();

        provider1.FromXmlString(_client.PrivateKey!);

        RsaSecurityKey rsaSecurityKey1 = new RsaSecurityKey(provider1);
        var signinCredentials = new SigningCredentials(rsaSecurityKey1, SecurityAlgorithms.RsaSha256);

        var expires = DateTime.UtcNow.AddSeconds(accessDuration);

        string access_token = JwtHelper.GenerateJwt("BurganIam", _client.returnuri, tokenClaims,
            expires: expires, signingCredentials: signinCredentials);

        var scopes = _tokenRequest.Scopes!.ToArray().Except(excludedScopes).ToList();
        //List<string> scopes = [];

        _tokenInfoDetail.TokenList.Add(JwtHelper.CreateTokenInfo(TokenType.AccessToken, _tokenInfoDetail.AccessTokenId, _client.id!, DateTime.UtcNow.AddSeconds(accessDuration), true, _user?.Reference ?? ""
        , scopes, _user?.Id ?? null, null, _consent?.id, _deviceId));

        return access_token;
    }

    private string CreateRefreshToken()
    {
        _tokenInfoDetail.RefreshTokenId = Guid.NewGuid();
        var refreshInfo = _client!.tokens!.FirstOrDefault(t => t.type == 1);

        var tokenClaims = new List<Claim>();
        if (refreshInfo != null)
        {
            tokenClaims.Add(new Claim("jti", _tokenInfoDetail.RefreshTokenId.ToString()));
            if (_user != null)
                tokenClaims.Add(new Claim("userId", _user!.Id.ToString()));
        }
        if (refreshInfo == null)
            return string.Empty;

        int refreshDuration = 0;
        try
        {
            refreshDuration = TimeHelper.ConvertStrDurationToSeconds(refreshInfo.duration!);
            if(_consent is not null)
            {
                var consentData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(_consent!.additionalData!);
                if(_consent.consentType!.Equals("OB_Account"))
                {
                    DateTime lastAccessDate = DateTime.Parse(consentData!.hspBlg.iznBlg.erisimIzniSonTrh.ToString());
                    var DateDiffAsSeconds = Convert.ToInt32((lastAccessDate.AddMilliseconds(-1) - _currentDate).TotalSeconds);
                    refreshDuration = DateDiffAsSeconds; 
                }
        
                if(_consent.consentType.Equals("OB_Payment"))
                {
                    refreshDuration = 15 * 24 * 60 * 60; // 15 day
                }
            }
            _tokenInfoDetail.RefreshTokenDuration = refreshDuration;
        }
        catch (FormatException ex)
        {
            Logger.LogError(ex.Message);
            return string.Empty;
        }

        RSACryptoServiceProvider provider1 = new RSACryptoServiceProvider();

        provider1.FromXmlString(_client.PrivateKey!);

        RsaSecurityKey rsaSecurityKey1 = new RsaSecurityKey(provider1);
        var signinCredentials = new SigningCredentials(rsaSecurityKey1, SecurityAlgorithms.RsaSha256);

        var refreshExpires = DateTime.UtcNow.AddSeconds(refreshDuration);
        string refresh_token = JwtHelper.GenerateJwt("BurganIam", _client.returnuri, tokenClaims,
            expires: refreshExpires, signingCredentials: signinCredentials);

        _tokenInfoDetail.TokenList.Add(JwtHelper.CreateTokenInfo(TokenType.RefreshToken, _tokenInfoDetail.RefreshTokenId, _client.id!, DateTime.UtcNow.AddSeconds(refreshDuration), true, _user?.Reference ?? ""
        , new List<string>(), _user?.Id ?? null, _tokenInfoDetail.AccessTokenId, _tokenRequest!.ConsentId, _deviceId));

        return refresh_token;
    }

    public async Task<TokenResponse> GenerateTokenResponse()
    {
     
        var tokenResponse = new TokenResponse()
        {
            TokenType = "Bearer",
            AccessToken = await CreateAccessToken(),
            ExpiresIn = _tokenInfoDetail.AccessTokenDuration
        };

        if(_tokenRequest.GrantType.Equals("refresh_token"))
        {
            tokenResponse.RefreshToken = _tokenRequest.RefreshToken;
            tokenResponse.RefreshTokenExpiresIn = Convert.ToInt32((_refreshTokenInfo.ExpiredAt.ToLocalTime() - _currentDate).TotalSeconds);
        }
        else
        {
            tokenResponse.RefreshToken = CreateRefreshToken();
            tokenResponse.RefreshTokenExpiresIn = _tokenInfoDetail.RefreshTokenDuration;
        }

        //openId Section
        if (!_tokenRequest.GrantType.Equals("client_credentials"))
        {
            if ((_tokenRequest!.Scopes?.Contains("openId") ?? false) || (_tokenRequest!.Scopes?.Contains("profile") ?? false))
            {
                tokenResponse.IdToken = await CreateIdToken();
            }
        }

        return tokenResponse;
    }

    public async Task<ServiceResponse<TokenResponse>> GenerateTokenWithRefreshToken(GenerateTokenRequest tokenRequest)
    {
        _tokenRequest = tokenRequest;
        
        var refreshTokenJti = JwtHelper.GetClaim(tokenRequest.RefreshToken!, "jti");
        if (refreshTokenJti == null)
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 404,
                Detail = "Refresh Token Not Found",
                Response = null
            };
        }
        var refreshTokenInfo = await _databaseContext.Tokens.FirstOrDefaultAsync(t =>
        t.Id == Guid.Parse(refreshTokenJti!) && t.TokenType == TokenType.RefreshToken);
        if (refreshTokenInfo == null)
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 404,
                Detail = "Refresh Token Not Found",
                Response = null
            };
        }

        if (!refreshTokenInfo.IsActive)
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 403,
                Detail = "Refresh Token is Not Valid",
                Response = null
            };
        }

        _refreshTokenInfo = refreshTokenInfo;

        var relatedToken = await _databaseContext.Tokens.FirstOrDefaultAsync(t =>
        t.Id == refreshTokenInfo.RelatedTokenId && t.TokenType == TokenType.AccessToken);
        if (relatedToken == null)
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 404,
                Detail = "Related Access Token Not Found",
                Response = null
            };
        }

        var idToken = await _databaseContext.Tokens.FirstOrDefaultAsync(t => t.RelatedTokenId == relatedToken.Id && t.TokenType == TokenType.IdToken);

        //OpenBanking Set Consent
        if(relatedToken.ConsentId is Guid)
        {
            if(!relatedToken.ConsentId.Equals(tokenRequest.ConsentId))
            {
                return new ServiceResponse<TokenResponse>()
                {
                    StatusCode = 484,
                    Detail = "Consent Mismatch",
                    Response = null
                };
            }

            var consent = await _consentService.GetConsent(relatedToken.ConsentId.Value);
            _consent = consent.Response;
        }

        if(idToken is {})
        {
            relatedToken.Scopes.Add("profile");
        }

        tokenRequest.Scopes = relatedToken.Scopes;

        var clientResponse = await _clientService.CheckClient(refreshTokenInfo.ClientId);

        if (clientResponse.StatusCode != 200)
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = clientResponse.StatusCode,
                Detail = clientResponse.Detail
            };
        }
        _client = clientResponse.Response;
        _transactionService.Client = _client;

        if (!_client!.allowedgranttypes!.Any(g => g.GrantType == tokenRequest.GrantType))
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 471,
                Detail = "Client Has No Authorize To Use Requested Grant Type"
            };
        }

        var requestedScopes = tokenRequest.Scopes.ToList();

        if (!requestedScopes.All(_client.allowedscopetags!.Contains))
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 473,
                Detail = "Client is Not Authorized For Requested Scopes"
            };
        }

        if (refreshTokenInfo.UserId != null)
        {
            var userResponse = await _userService.GetUserById(refreshTokenInfo.UserId!.Value);

            if (userResponse.StatusCode != 200)
            {
                return new ServiceResponse<TokenResponse>()
                {
                    StatusCode = userResponse.StatusCode,
                    Detail = userResponse.Detail
                };
            }

            _user = userResponse.Response;

            _collectionUser = _collectionUsers.Users.FirstOrDefault(u => u.CitizenshipNo == _user!.Reference);

            var profile = await _profileService!.GetCustomerSimpleProfile(refreshTokenInfo.Reference!);
            if (profile.StatusCode != 200)
            {
                _profile = null;
            }
            else
            {
                _profile = profile.Response;
            }
            

            if (_user?.State.ToLower() != "active" && _user?.State.ToLower() != "new")
            {
                return new ServiceResponse<TokenResponse>()
                {
                    StatusCode = 470,
                    Detail = "User is disabled"
                };
            }

            var dodgeUserResponse = await _internetBankingUserService.GetUser(refreshTokenInfo.Reference!);
            if (dodgeUserResponse.StatusCode != 200)
            {
                _transactionService.RoleKey = 20;
            }
            else
            {
                var dodgeUser = dodgeUserResponse.Response;

                var role = await _ibContext.Role.Where(r => r.UserId.Equals(dodgeUser!.Id) && r.Channel.Equals(10) && r.Status.Equals(10)).OrderByDescending(r => r.CreatedAt).FirstOrDefaultAsync();
                if(role is {} && (role.ExpireDate ?? DateTime.MaxValue) > DateTime.Now)
                {
                    var roleDefinition = await _ibContext.RoleDefinition.FirstOrDefaultAsync(d => d.Id.Equals(role.DefinitionId) && d.IsActive);
                    if(roleDefinition is {})
                    {
                        if(roleDefinition.Key == 0)
                        {  
                            return new ServiceResponse<TokenResponse>()
                            {
                                StatusCode = 471,
                                Detail = ErrorHelper.GetErrorMessage(LoginErrors.NotAuthorized, "en-EN")
                            };
                            
                        }
                        else
                        {
                            _transactionService.RoleKey = roleDefinition.Key;
                        }
                    }
                }
            }
        }
        else
        {
            _user = null;
            _tokenRequest.GrantType = "client_credentials";
        }

        RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
        provider.FromXmlString(_client.PublicKey!);
            
        RsaSecurityKey rsaSecurityKey = new RsaSecurityKey(provider);

        JwtSecurityToken? refreshTokenValidated;
        
        if (JwtHelper.ValidateToken(tokenRequest.RefreshToken!, "BurganIam", _client.returnuri, rsaSecurityKey, out refreshTokenValidated))
        {

            var tokenResponse = await GenerateTokenResponse();

            if (tokenResponse.AccessToken == string.Empty || tokenResponse.RefreshToken == string.Empty)
            {
                return new ServiceResponse<TokenResponse>()
                {
                    StatusCode = 500,
                    Detail = "Access Token Or Refresh Token Couldn't Be Generated"
                };
            }

            await PersistTokenInfo();

            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 200,
                Response = tokenResponse
            };
        }
        else
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 403,
                Detail = "Token Validation Error",
                Response = null
            };
        }

    }

    public async Task<ServiceResponse<TokenResponse>> GenerateTokenWithPasswordFromWorkflow(GenerateTokenRequest tokenRequest, ClientResponse client, LoginResponse user, SimpleProfileResponse? profile, string deviceId = "")
    {
        _tokenRequest = tokenRequest;

        _client = client;
        _transactionService.Client = _client;
        _user = user;
        _profile = profile;
        _deviceId = deviceId;

        var consentListResponse = await _roleService!.GetConsents(_client!.id!, _user.Reference);
        if(consentListResponse.StatusCode == 200)
        {
            var consentList = consentListResponse.Response;

            var consent = consentList!.FirstOrDefault();
            _selectedConsent = consent;

            var roleResponse = await _roleService.GetRole(consent!.RoleId);
            if(roleResponse.StatusCode == 200)
            {
                var amorphieRole = roleResponse.Response;
                _role = amorphieRole;
                var roleDefinition = await _roleService.GetRoleDefinition(_role!.DefinitionId);
                if(roleDefinition.StatusCode == 200)
                {
                    _roleDefinition = roleDefinition.Response;
                }
            }
            
        }
        

        var tokenResponse = await GenerateTokenResponse();

        if (tokenResponse.IdToken == string.Empty && tokenResponse.AccessToken == string.Empty)
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 500,
                Detail = "Access Token And Id Token Couldn't Be Generated"
            };
        }

        await _databaseContext.Tokens.Where(t => t.Reference == _tokenRequest.Username && t.IsActive).ExecuteUpdateAsync(s => s.SetProperty(t => t.IsActive, false));
        await PersistTokenInfo();

        return new ServiceResponse<TokenResponse>()
        {
            StatusCode = 200,
            Response = tokenResponse
        };
    }
    
    public async Task<ServiceResponse<TokenResponse>> GenerateTokenWithDevice(GenerateTokenRequest tokenRequest)
    {
        _tokenRequest = tokenRequest;
        var checkDeviceResponse = await _userService.CheckDeviceWithoutUser(_tokenRequest.ClientId, _tokenRequest.DeviceId, Guid.Parse(_tokenRequest.InstallationId));
        if (checkDeviceResponse.StatusCode != 200)
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = checkDeviceResponse.StatusCode,
                Detail = checkDeviceResponse.Detail
            };
        }
        if (!_tokenRequest.Username.Equals(checkDeviceResponse.Response.Reference))
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 490,
                Detail = "User Has No Authorize On Requested Device"
            };
        }

        ServiceResponse<ClientResponse> clientResponse;
        if (Guid.TryParse(_tokenRequest.ClientId!, out Guid _))
        {
            clientResponse = await _clientService.ValidateClient(_tokenRequest.ClientId!, _tokenRequest.ClientSecret!);
        }
        else
        {
            clientResponse = await _clientService.ValidateClientByCode(_tokenRequest.ClientId!, _tokenRequest.ClientSecret!);
        }

        _client = clientResponse.Response;
        _transactionService.Client = _client;

        if (!_client!.allowedgranttypes!.Any(g => g.GrantType == tokenRequest.GrantType))
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 471,
                Detail = "Client Has No Authorize To Use Requested Grant Type"
            };
        }

        var requestedScopes = tokenRequest.Scopes!.ToList();

        if (!requestedScopes.All(_client.allowedscopetags!.Contains))
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 473,
                Detail = "Client is Not Authorized For Requested Scopes"
            };
        }

        var userResponse = await _internetBankingUserService.GetUser(_tokenRequest.Username!);
        if (userResponse.StatusCode != 200)
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 404,
                Detail = "User Not Found"
            };
        }
        var user = userResponse.Response;

        var userStatus = await _ibContext.Status.Where(s => s.UserId == user!.Id && (!s.State.HasValue || s.State.Value == 10)).OrderByDescending(s => s.CreatedAt).FirstOrDefaultAsync();
        if (userStatus?.Type == 30 || userStatus?.Type == 40)
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 404,
                Detail = "User Not Active"
            };
        }

        var userInfoResult = await _profileService.GetCustomerSimpleProfile(_tokenRequest.Username!);
        if (userInfoResult.StatusCode != 200)
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 471,
                Detail = "User Profile Couldn't Be Fetched"
            };
        }

        var userInfo = userInfoResult.Response;
        _profile = userInfo;
        if (userInfo!.data!.profile!.Equals("customer") || !userInfo!.data!.profile!.status!.Equals("active"))
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 471,
                Detail = "User is Not Customer Or Not Active"
            };
        }

        var mobilePhoneCount = userInfo!.data!.phones!.Count(p => p.type!.Equals("mobile"));
        if (mobilePhoneCount != 1)
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 471,
                Detail = "Bad Phone Data"
            };
        }

        var mobilePhone = userInfo!.data!.phones!.FirstOrDefault(p => p.type!.Equals("mobile"));
        if (string.IsNullOrWhiteSpace(mobilePhone!.prefix) || string.IsNullOrWhiteSpace(mobilePhone!.number))
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 471,
                Detail = "Bad Phone Format"
            };
        }

        var userRequest = new UserInfo
        {
            firstName = userInfo!.data.profile!.name!,
            lastName = userInfo!.data.profile!.name!,
            phone = new core.Models.User.UserPhone()
            {
                countryCode = mobilePhone!.countryCode!,
                prefix = mobilePhone!.prefix,
                number = mobilePhone!.number
            },
            state = "Active",
            password = _tokenRequest.Password!,
            explanation = "Migrated From IB",
            reason = "Amorphie Login"
        };
        var verifiedMailAddress = userInfo.data.emails!.FirstOrDefault(m => m.isVerified == true);
        userRequest.eMail = verifiedMailAddress?.address ?? "";
        userRequest.reference = _tokenRequest.Username!;

        var migrateResult = await _userService.SaveUser(userRequest);

        var userAmorphieResponse = await _userService.Login(new LoginRequest() { Reference = tokenRequest.Username!, Password = tokenRequest.Password! });

        if (userAmorphieResponse.StatusCode != 200)
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = userResponse.StatusCode,
                Detail = userResponse.Detail
            };
        }

        _user = userAmorphieResponse.Response;

        if (_user?.State.ToLower() != "active" && _user?.State.ToLower() != "new")
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 470,
                Detail = "User is disabled"
            };
        }

        var tokenResponse = await GenerateTokenResponse();

        if (tokenResponse.IdToken == string.Empty && tokenResponse.AccessToken == string.Empty)
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 500,
                Detail = "Access Token And Id Token Couldn't Be Generated"
            };
        }

        await PersistTokenInfo();

        return new ServiceResponse<TokenResponse>()
        {
            StatusCode = 200,
            Response = tokenResponse
        };
    }

    public async Task<ServiceResponse<TokenResponse>> GenerateTokenWithPassword(GenerateTokenRequest tokenRequest)
    {
        _tokenRequest = tokenRequest;
        ServiceResponse<ClientResponse> clientResponse;
        if (Guid.TryParse(_tokenRequest.ClientId!, out Guid _))
        {
            clientResponse = await _clientService.ValidateClient(_tokenRequest.ClientId!, _tokenRequest.ClientSecret!);
        }
        else
        {
            clientResponse = await _clientService.ValidateClientByCode(_tokenRequest.ClientId!, _tokenRequest.ClientSecret!);
        }

        if (clientResponse.StatusCode != 200)
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = clientResponse.StatusCode,
                Detail = clientResponse.Detail
            };
        }

        _client = clientResponse.Response;
        _transactionService.Client = _client;

        if (!_client!.allowedgranttypes!.Any(g => g.GrantType == tokenRequest.GrantType))
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 471,
                Detail = "Client Has No Authorize To Use Requested Grant Type"
            };
        }

        var requestedScopes = tokenRequest.Scopes!.ToList();

        if (!requestedScopes.All(_client.allowedscopetags!.Contains))
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 473,
                Detail = "Client is Not Authorized For Requested Scopes"
            };
        }
    
        var userResponse = await _internetBankingUserService.GetUser(_tokenRequest.Username!);
        if (userResponse.StatusCode != 200)
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 404,
                Detail = "User Not Found"
            };
        }
        var user = userResponse.Response;

        var userStatus = await _ibContext.Status.Where(s => s.UserId == user!.Id && (!s.State.HasValue || s.State.Value == 10)).OrderByDescending(s => s.CreatedAt).FirstOrDefaultAsync();

        var passwordResponse = await _internetBankingUserService.GetPassword(user!.Id);
        if (userResponse.StatusCode != 200)
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 404,
                Detail = "User Not Found"
            };
        }
        var passwordRecord = passwordResponse.Response;

        var isVerified = _internetBankingUserService.VerifyPassword(passwordRecord!.HashedPassword!, _tokenRequest.Password!, passwordRecord.Id.ToString());
        //Consider SuccessRehashNeeded
        if (isVerified != PasswordVerificationResult.Success)
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 471,
                Detail = "Password Doesn't Match"
            };
        }

        var role = await _ibContext.Role.Where(r => r.UserId.Equals(user!.Id) && r.Channel.Equals(10) && r.Status.Equals(10)).OrderByDescending(r => r.CreatedAt).FirstOrDefaultAsync();
        if(role is {} && (role.ExpireDate ?? DateTime.MaxValue) > DateTime.Now)
        {
            var roleDefinition = await _ibContext.RoleDefinition.FirstOrDefaultAsync(d => d.Id.Equals(role.DefinitionId) && d.IsActive);
            if(roleDefinition is {})
            {
                if(roleDefinition.Key == 0)
                {  
                    return new ServiceResponse<TokenResponse>()
                    {
                        StatusCode = 471,
                        Detail = ErrorHelper.GetErrorMessage(LoginErrors.NotAuthorized, "en-EN")
                    };
                    
                }
                else
                {
                    _transactionService.RoleKey = roleDefinition.Key;
                }
            }
        }

        var userInfoResult = await _profileService.GetCustomerSimpleProfile(_tokenRequest.Username!);
        if (userInfoResult.StatusCode != 200)
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 471,
                Detail = "User Profile Couldn't Be Fetched"
            };
        }

        var userInfo = userInfoResult.Response;
        _profile = userInfo;
        if (userInfo!.data!.profile!.Equals("customer") || !userInfo!.data!.profile!.status!.Equals("active"))
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 471,
                Detail = "User is Not Customer Or Not Active"
            };
        }

        var mobilePhoneCount = userInfo!.data!.phones!.Count(p => p.type!.Equals("mobile"));
        if (mobilePhoneCount != 1)
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 471,
                Detail = "Bad Phone Data"
            };
        }

        var mobilePhone = userInfo!.data!.phones!.FirstOrDefault(p => p.type!.Equals("mobile"));
        if (string.IsNullOrWhiteSpace(mobilePhone!.prefix) || string.IsNullOrWhiteSpace(mobilePhone!.number))
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 471,
                Detail = "Bad Phone Format"
            };
        }

        var userRequest = new UserInfo
        {
            firstName = userInfo!.data.profile!.name!,
            lastName = userInfo!.data.profile!.name!,
            phone = new core.Models.User.UserPhone()
            {
                countryCode = mobilePhone!.countryCode!,
                prefix = mobilePhone!.prefix,
                number = mobilePhone!.number
            },
            state = "Active",
            salt = passwordRecord.Id.ToString(),
            password = _tokenRequest.Password!,
            explanation = "Migrated From IB",
            reason = "Amorphie Login",
            isArgonHash = true
        };
        var verifiedMailAddress = userInfo.data.emails!.FirstOrDefault(m => m.isVerified == true);
        userRequest.eMail = verifiedMailAddress?.address ?? "";
        userRequest.reference = _tokenRequest.Username!;

        var migrateResult = await _userService.SaveUser(userRequest);

        var userAmorphieResponse = await _userService.Login(new LoginRequest() { Reference = tokenRequest.Username!, Password = tokenRequest.Password! });

        if (userAmorphieResponse.StatusCode != 200)
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = userResponse.StatusCode,
                Detail = userResponse.Detail
            };
        }

        _user = userAmorphieResponse.Response;

        if (_user?.State.ToLower() != "active" && _user?.State.ToLower() != "new")
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 470,
                Detail = "User is disabled"
            };
        }
        
        var tokenResponse = await GenerateTokenResponse();

        if (tokenResponse.IdToken == string.Empty && tokenResponse.AccessToken == string.Empty)
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 500,
                Detail = "Access Token And Id Token Couldn't Be Generated"
            };
        }

        await PersistTokenInfo();

        return new ServiceResponse<TokenResponse>()
        {
            StatusCode = 200,
            Response = tokenResponse
        };
    }

    public async Task<ServiceResponse<TokenResponse>> GenerateTokenWithSystemUser(GenerateTokenRequest tokenRequest)
    {
        _tokenRequest = tokenRequest;
        ServiceResponse<ClientResponse> clientResponse;
        if (Guid.TryParse(_tokenRequest.ClientId!, out Guid _))
        {
            clientResponse = await _clientService.ValidateClient(_tokenRequest.ClientId!, _tokenRequest.ClientSecret!);
        }
        else
        {
            clientResponse = await _clientService.ValidateClientByCode(_tokenRequest.ClientId!, _tokenRequest.ClientSecret!);
        }

        if (clientResponse.StatusCode != 200)
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = clientResponse.StatusCode,
                Detail = clientResponse.Detail
            };
        }

        _client = clientResponse.Response;
        _transactionService.Client = _client;

        if (!_client!.allowedgranttypes!.Any(g => g.GrantType == tokenRequest.GrantType))
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 471,
                Detail = "Client Has No Authorize To Use Requested Grant Type"
            };
        }

        var requestedScopes = tokenRequest.Scopes!.ToList();

        if (!requestedScopes.All(_client.allowedscopetags!.Contains))
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 473,
                Detail = "Client is Not Authorized For Requested Scopes"
            };
        }

        var user = await _ibSecurityContext.AspNetUsers.AsNoTracking().FirstOrDefaultAsync(u => u.UserName.Equals(_tokenRequest.Username));
        if(user is not {})
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 404,
                Detail = "User Not Found"
            };
        }

        var passwordHasher = new PasswordHasher();
        var isVerified = passwordHasher.VerifyHashedPassword(user!.PasswordHash!, _tokenRequest.Password!);
        //Consider SuccessRehashNeeded
        if (isVerified != PasswordVerificationResult.Success)
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 471,
                Detail = "Password Doesn't Match"
            };
        }

        var tokenResponse = await GenerateTokenResponse();

        if (tokenResponse.IdToken == string.Empty && tokenResponse.AccessToken == string.Empty)
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 500,
                Detail = "Access Token And Id Token Couldn't Be Generated"
            };
        }

        await PersistTokenInfo();

        return new ServiceResponse<TokenResponse>()
        {
            StatusCode = 200,
            Response = tokenResponse
        };
    }

    public async Task<ServiceResponse<TokenResponse>> GenerateTokenWithClientCredentials(GenerateTokenRequest tokenRequest)
    {
        _tokenRequest = tokenRequest;
        _user = null;
        
        ServiceResponse<ClientResponse> clientResponse;
        if (Guid.TryParse(_tokenRequest.ClientId!, out Guid _))
        {
            clientResponse = await _clientService.ValidateClient(_tokenRequest.ClientId!, _tokenRequest.ClientSecret!);
        }
        else
        {
            clientResponse = await _clientService.ValidateClientByCode(_tokenRequest.ClientId!, _tokenRequest.ClientSecret!);
        }

        if (clientResponse.StatusCode != 200)
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = clientResponse.StatusCode,
                Detail = clientResponse.Detail
            };
        }

        _client = clientResponse.Response;
        _transactionService.Client = _client;

        if (!_client!.allowedgranttypes!.Any(g => g.GrantType == tokenRequest.GrantType))
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 471,
                Detail = "Client Has No Authorize To Use Requested Grant Type"
            };
        }

        var requestedScopes = tokenRequest.Scopes!.ToList();

        if (!requestedScopes.All(_client.allowedscopetags!.Contains))
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 473,
                Detail = "Client is Not Authorized For Requested Scopes"
            };
        }

        var tokenResponse = await GenerateTokenResponse();

        if (tokenResponse.IdToken == string.Empty && tokenResponse.AccessToken == string.Empty)
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 500,
                Detail = "Access Token And Id Token Couldn't Be Generated"
            };
        }

        await PersistTokenInfo();

        return new ServiceResponse<TokenResponse>()
        {
            StatusCode = 200,
            Response = tokenResponse
        };
    }

    public async Task<ServiceResponse<TokenResponse>> GenerateToken(GenerateTokenRequest tokenRequest)
    {
        _tokenRequest = tokenRequest;
        ServiceResponse<ClientResponse> clientResponse;

        if(!string.IsNullOrWhiteSpace(tokenRequest.ClientSecret))
        {
            if(Guid.TryParse(tokenRequest.ClientId, out Guid _))
            {
                clientResponse = await _clientService.ValidateClient(tokenRequest.ClientId!, tokenRequest.ClientSecret!);
            }
            else
            {
                clientResponse = await _clientService.ValidateClientByCode(tokenRequest.ClientId!, tokenRequest.ClientSecret!);
            }
            if (clientResponse.StatusCode != 200)
            {
                return new ServiceResponse<TokenResponse>()
                {
                    StatusCode = clientResponse.StatusCode,
                    Detail = clientResponse.Detail
                };
            }
        }
        else
        {
            if(Guid.TryParse(tokenRequest.ClientId, out Guid _))
            {
                clientResponse = await _clientService.CheckClient(tokenRequest.ClientId!);
            }
            else
            {
                clientResponse = await _clientService.CheckClientByCode(tokenRequest.ClientId!);
            }
            if (clientResponse.StatusCode != 200)
            {
                return new ServiceResponse<TokenResponse>()
                {
                    StatusCode = clientResponse.StatusCode,
                    Detail = clientResponse.Detail
                };
            }
        }

        _client = clientResponse.Response;
        _transactionService.Client = _client;

        if (!_client!.allowedgranttypes!.Any(g => g.GrantType == tokenRequest.GrantType))
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 471,
                Detail = "Client Has No Authorize To Use Requested Grant Type"
            };
        }
        
        var authorizationCodeInfo = await _daprClient.GetStateAsync<AuthorizationCode>(Configuration["DAPR_STATE_STORE_NAME"], tokenRequest.Code);

        if (authorizationCodeInfo == null)
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 470,
                Detail = "Invalid Authorization Code"
            };
        }
        
        _user = authorizationCodeInfo.Subject;
        _profile = authorizationCodeInfo.Profile;
        _collectionUser = authorizationCodeInfo.CollectionUser;
        _tokenRequest.Scopes = authorizationCodeInfo.RequestedScopes;

        if (!authorizationCodeInfo.ClientId!.Equals(_client.id, StringComparison.OrdinalIgnoreCase))
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 472,
                Detail = "ClientId Not Matched"
            };
        }

        if (_client.pkce == "must")
        {
            if (!authorizationCodeInfo.CodeChallenge!.Equals(GetHashedCodeVerifier(tokenRequest.CodeVerifier!)))
            {
                return new ServiceResponse<TokenResponse>()
                {
                    StatusCode = 473,
                    Detail = "Code Verifier Not Matched"
                };
            }
        }

        var dodgeUserResponse = await _internetBankingUserService.GetUser(_user.Reference!);
        if (dodgeUserResponse.StatusCode != 200)
        {
            _transactionService.RoleKey = 20;
        }
        else
        {
            var dodgeUser = dodgeUserResponse.Response;

            var role = await _ibContext.Role.Where(r => r.UserId.Equals(dodgeUser!.Id) && r.Channel.Equals(10) && r.Status.Equals(10)).OrderByDescending(r => r.CreatedAt).FirstOrDefaultAsync();
            if(role is {} && (role.ExpireDate ?? DateTime.MaxValue) > DateTime.Now)
            {
                var roleDefinition = await _ibContext.RoleDefinition.FirstOrDefaultAsync(d => d.Id.Equals(role.DefinitionId) && d.IsActive);
                if(roleDefinition is {})
                {
                    if(roleDefinition.Key == 0)
                    {  
                        return new ServiceResponse<TokenResponse>()
                        {
                            StatusCode = 471,
                            Detail = ErrorHelper.GetErrorMessage(LoginErrors.NotAuthorized, "en-EN")
                        };
                        
                    }
                    else
                    {
                        _transactionService.RoleKey = roleDefinition.Key;
                    }
                }
            }
        }

        var tokenResponse = await GenerateTokenResponse();

        if (tokenResponse.IdToken == string.Empty && tokenResponse.AccessToken == string.Empty)
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 500,
                Detail = "Access Token And Id Token Couldn't Be Generated"
            };
        }

        await PersistTokenInfo();
        return new ServiceResponse<TokenResponse>()
        {
            StatusCode = 200,
            Response = tokenResponse
        };
    }

    public async Task<ServiceResponse<TokenResponse>> GenerateOpenBankingToken(GenerateTokenRequest tokenRequest, ConsentResponse consent)
    {
        _tokenRequest = tokenRequest;
        _consent = consent;

        var clientResponse = await _clientService.ValidateClient(tokenRequest.ClientId!, tokenRequest.ClientSecret!);
        if (clientResponse.StatusCode != 200)
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = clientResponse.StatusCode,
                Detail = clientResponse.Detail
            };
        }

        _client = clientResponse.Response;
        _transactionService.Client = _client;

        if (!_client!.allowedgranttypes!.Any(g => g.GrantType == tokenRequest.GrantType))
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 471,
                Detail = "Client Has No Authorize To Use Requested Grant Type"
            };
        }

        var authorizationCodeInfo = await _daprClient.GetStateAsync<AuthorizationCode>(Configuration["DAPR_STATE_STORE_NAME"], tokenRequest.Code);

        if (authorizationCodeInfo == null)
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 470,
                Detail = "Invalid Authorization Code"
            };
        }

        _user = authorizationCodeInfo.Subject;

        if (!authorizationCodeInfo.ClientId!.Equals(tokenRequest.ClientId, StringComparison.OrdinalIgnoreCase))
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 471,
                Detail = "ClientId Not Matched"
            };
        }

        if (_client.pkce == "must")
        {
            if (!authorizationCodeInfo.CodeChallenge!.Equals(GetHashedCodeVerifier(tokenRequest.CodeVerifier!)))
            {
                return new ServiceResponse<TokenResponse>()
                {
                    StatusCode = 472,
                    Detail = "Code Verifier Not Matched"
                };
            }
        }

        var tokenResponse = await GenerateTokenResponse();
        tokenResponse.scope = string.Join(" ",authorizationCodeInfo!.RequestedScopes!);

        if (tokenResponse.IdToken == string.Empty && tokenResponse.AccessToken == string.Empty)
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 500,
                Detail = "Access Token And Id Token Couldn't Be Generated"
            };
        }

        await PersistTokenInfo();
        return new ServiceResponse<TokenResponse>()
        {
            StatusCode = 200,
            Response = tokenResponse
        };
    }

    private string GetHashedCodeVerifier(string codeVerifier)
    {
        var codeVerifierAsByte = System.Text.Encoding.ASCII.GetBytes(codeVerifier);

        using var sha256 = SHA256.Create();
        var hashedCodeVerifier = Base64UrlEncoder.Encode(sha256.ComputeHash(codeVerifierAsByte));

        return hashedCodeVerifier;
    }

    private List<string> GetClientClaims(List<string> clientClaims)
    {
        return clientClaims.Where(c => !IsUserBasedClaim(c)).ToList();
    }

    private string GetClaimSourcePath(string claim)
    {
        var claimInfoList = claim.Split("|");
        if(claimInfoList.Count() > 1)
        {
            return claimInfoList[1];
        }
        else
        {
            return claimInfoList[0];
        }
    }

    private bool IsUserBasedClaim(string claim)
    {
        var claimSource = GetClaimSourcePath(claim);
        return GetClaimInfoSource(claimSource) switch 
            {
                "const" => false,
                _ => true
            };
    }

    private string GetClaimInfoSource(string claimInfoPath)
    {
        var claimInfoPathList = claimInfoPath.Split(".");
        return claimInfoPathList[0];
    }

    public async Task<ServiceResponse<TokenResponse>> GenerateTokenWithRefreshTokenFromWorkflowAsync(TokenInfo refreshTokenInfo, ClientResponse client, LoginResponse user, SimpleProfileResponse? profile, IBUser? dodgeUser, int? dodgeRoleKey, ConsentResponse? consent)
    {
        _client = client;
        _user = user;
        _profile = profile;
        _consent = consent;
        _transactionService.RoleKey = dodgeRoleKey.HasValue ? dodgeRoleKey.Value : 0;
        _transactionService.Client = client;
        _refreshTokenInfo = refreshTokenInfo;

        var consentListResponse = await _roleService!.GetConsents(_client!.id!, _user.Reference);
        if(consentListResponse.StatusCode == 200)
        {
            var consentList = consentListResponse.Response;

            var selectedConsent = consentList!.FirstOrDefault();
            _selectedConsent = selectedConsent;

            var roleResponse = await _roleService.GetRole(selectedConsent!.RoleId);
            if(roleResponse.StatusCode == 200)
            {
                var amorphieRole = roleResponse.Response;
                _role = amorphieRole;
                var roleDefinition = await _roleService.GetRoleDefinition(_role!.DefinitionId);
                if(roleDefinition.StatusCode == 200)
                {
                    _roleDefinition = roleDefinition.Response;
                }
            }
            
        }

        _tokenRequest = new GenerateTokenRequest{
             GrantType = "refresh_token"
        };

        var tokenResponse = await GenerateTokenResponse();

        if (tokenResponse.AccessToken == string.Empty || tokenResponse.RefreshToken == string.Empty)
        {
            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 500,
                Detail = "Access Token Or Refresh Token Couldn't Be Generated"
            };
        }

        await PersistTokenInfo();

        return new ServiceResponse<TokenResponse>()
        {
            StatusCode = 200,
            Response = tokenResponse
        };

    }
}
