using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using amorphie.token.data;
using amorphie.token.Services.InternetBanking;
using amorphie.token.Services.Profile;
using amorphie.token.Services.FlowHandler;
using amorphie.token.Services.Consent;
using amorphie.token.Services.TransactionHandler;
using amorphie.token.Services.Login;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using amorphie.token.Services.Role;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Web;
using Dapr.Extensions.Configuration;
using Google.Protobuf.WellKnownTypes;
using amorphie.token.core.Dtos;


namespace amorphie.token.core.Controllers;

public class AuthorizeController : Controller
{
    private readonly ILogger<AuthorizeController> _logger;
    private readonly IAuthorizationService _authorizationService;
    private readonly IUserService _userService;
    private readonly IClientService _clientService;
    private readonly IInternetBankingUserService _ibUserService;
    private readonly DatabaseContext _databaseContext;
    private readonly IConfiguration _configuration;
    private readonly IFlowHandler _flowHandler;
    private readonly DaprClient _daprClient;
    private readonly ITransactionService _transactionService;
    private readonly IConsentService _consentService;
    private readonly IProfileService _profileService;
    private readonly ILoginService _loginService;
    private readonly IRoleService _roleService;
    private readonly CollectionUsers _collectionUsers;
    
    public AuthorizeController(ILogger<AuthorizeController> logger, IAuthorizationService authorizationService, IUserService userService, DatabaseContext databaseContext
    , IConfiguration configuration, DaprClient daprClient, IClientService clientService, IInternetBankingUserService ibUserService, ITransactionService transactionService,
    IFlowHandler flowHandler, CollectionUsers collectionUsers, IRoleService roleService, IConsentService consentService, IProfileService profileService, ILoginService loginService)
    {
        _logger = logger;
        _authorizationService = authorizationService;
        _userService = userService;
        _databaseContext = databaseContext;
        _configuration = configuration;
        _flowHandler = flowHandler;
        _clientService = clientService;
        _ibUserService = ibUserService;
        _transactionService = transactionService;
        _daprClient = daprClient;
        _consentService = consentService;
        _profileService = profileService;
        _loginService = loginService;
        _roleService = roleService;
        _collectionUsers = collectionUsers;
    }

    [HttpGet("/private/GenerateCollectionClaims")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GenerateCollectionClaims()
    {
        Dictionary<string,Dictionary<string,string>> response = new();
        try
        {
            
            var users = _collectionUsers.Users;

            List<SaveUserClaimDto> claimList = new List<SaveUserClaimDto>();
            foreach(var user in users)
            {
                string lastKey = string.Empty;
                Dictionary<string,string> claimStatusList = new();
                try
                {
                    var toAdd = await CreateDto(user);
                    foreach(var item in toAdd)
                    {
                        lastKey = item.ClaimName;
                        using var httpClient = new HttpClient();
                        var res = await httpClient.PostAsJsonAsync(_configuration["UserBaseAddress"]+"userClaim",item);
                        if(res.IsSuccessStatusCode)
                        {
                            claimStatusList.Add(lastKey,"success");
                        }
                        else
                        {
                            claimStatusList.Add(lastKey,"failed | " + await res.Content.ReadAsStringAsync());
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    claimStatusList.Add(lastKey,"failed | " + ex.ToString());
                }
                response.Add(user.CitizenshipNo, claimStatusList);
            }

            return Ok(response);
        }
        catch (System.Exception ex)
        {
            return Ok(response);
        }
        
        
    }

    private async Task<List<SaveUserClaimDto>> CreateDto(core.Models.Collection.User user)
    {
        var userReponse = await _userService.GetUserByReference(user.CitizenshipNo);
        List<SaveUserClaimDto> toAdd =
        [
            new SaveUserClaimDto{
                UserId = userReponse.Response.Id.ToString(),
                ClaimName = "LoginUser",
                ClaimValue = user.LoginUser
            },
            new SaveUserClaimDto{
                UserId = userReponse.Response.Id.ToString(),
                ClaimName = "DepartmentCode",
                ClaimValue = user.DepartmentCode
            },
            new SaveUserClaimDto{
                UserId = userReponse.Response.Id.ToString(),
                ClaimName = "Role",
                ClaimValue = user.Role.GetHashCode().ToString()
            },
            new SaveUserClaimDto{
                UserId = userReponse.Response.Id.ToString(),
                ClaimName = "Client",
                ClaimValue = "collection"
            },
        ];

        return toAdd;
    }

    
    
    [HttpGet("/private/Logout")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> Logout([FromQuery] string token)
    {
        

        return Ok();

    }

    
    [HttpPost("/post-transition/{recordId}/{transition}")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> PostTransition([FromRoute] string recordId, [FromRoute] string transition, [FromBody] dynamic body)
    {
        using var httpClient = new HttpClient();
        var content1 = new StringContent(JsonSerializer.Serialize(new{
            grant_type = "client_credentials",
            client_id = "IbAndroidApp",
            client_secret = "",
            scopes = new string[] { "openId","retail-customer"}
        }),Encoding.UTF8,"application/json");
        var resp2 = await httpClient.PostAsync("https://test-pubagw6.burgan.com.tr/ebanking/token",content1);
        var res1 = await resp2.Content.ReadAsStringAsync();
        
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(res1);

        var FlowUserInfo = JsonSerializer.Deserialize<FlowUserInfo>(HttpContext.Session.GetString("FlowUserInfo"));

        var content = new StringContent(JsonSerializer.Serialize(body),Encoding.UTF8,"application/json");
        httpClient.DefaultRequestHeaders.Add("Authorization","Bearer "+tokenResponse.AccessToken);
        httpClient.DefaultRequestHeaders.Add("User",Guid.NewGuid().ToString());
        httpClient.DefaultRequestHeaders.Add("Behalf-Of-User",Guid.NewGuid().ToString());
        httpClient.DefaultRequestHeaders.Add("xdeviceid",FlowUserInfo.deviceId);
        httpClient.DefaultRequestHeaders.Add("xtokenid",FlowUserInfo.tokenId);
        httpClient.DefaultRequestHeaders.Add("xdeployment","ANDROID");
        httpClient.DefaultRequestHeaders.Add("xdeviceinfo","SAMSUNG");
        var resp = await httpClient.PostAsync($"https://test-amorphie-workflow.burgan.com.tr/workflow/instance/{recordId}/transition/{transition}",content);
        var res = await resp.Content.ReadAsStringAsync();

        return Ok();
    }

    [HttpPost("/get-page")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GetPage([FromBody] dynamic body)
    {
        var page_id = body.GetProperty("page_id").ToString();
        if(page_id == "AMORPHIE_LOGIN_PAGE")
        {
            return View("InnerLoginPage",new Login());
        }
        if(page_id == "OTP")
        {
            return View("Otp",new Otp());
        }
        return StatusCode(403);
    }


    [HttpGet("/public/flow/demo")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> FlowDemo()
    {
        ViewBag.deviceId = Guid.NewGuid().ToString();
        ViewBag.tokenId = Guid.NewGuid().ToString();
        ViewBag.requestId = Guid.NewGuid().ToString();

        HttpContext.Session.SetString("FlowUserInfo",JsonSerializer.Serialize(new FlowUserInfo(){
            deviceId = ViewBag.deviceId,
            tokenId = ViewBag.tokenId
        }));

        using var httpClient = new HttpClient();
        var content = new StringContent(JsonSerializer.Serialize(new{
            grant_type = "client_credentials",
            client_id = "IbAndroidApp",
            client_secret = "",
            scopes = new string[] { "openId","retail-customer"}
        }),Encoding.UTF8,"application/json");
        var resp = await httpClient.PostAsync("https://test-pubagw6.burgan.com.tr/ebanking/token",content);
        var res = await resp.Content.ReadAsStringAsync();
        
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(res);
        ViewBag.accessToken = tokenResponse!.AccessToken;

        return View("LoginFlowPage");
    }

    [HttpGet("/public/getAuthorize")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GetAuthorize()
    {
        await Task.CompletedTask;
        return Redirect("http://localhost:4900/public/Authorize?response_type=code&client_id=IbAndroidApp&state=test&scope=profile&redirect_uri=tt");
    }

    [HttpPost("/public/get-user-info")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GetUserInfo([FromForm] string access_token)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",access_token);
        var response = await httpClient.GetAsync(_configuration["GetUserInfoAddress"]);
        if(response.IsSuccessStatusCode)
        {
            var t = await response.Content.ReadAsStringAsync();
            var r = Newtonsoft.Json.JsonConvert.DeserializeObject<CustomerEntity>(await response.Content.ReadAsStringAsync());

            return new OkObjectResult(r);
        }
        else
        {
            return StatusCode((int)response.StatusCode);
        }
    }

    [HttpGet("/private/get-authorized-user-info")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GetAuthorizedUserInfo()
    {
        await Task.CompletedTask;
        var obj = new{
                    TCKN = HttpContext.Request.Headers.FirstOrDefault(h => h.Key == "User_reference").Value.ToString(),
                    BusinessLine = HttpContext.Request.Headers.FirstOrDefault(h => h.Key == "Business_line").Value.ToString(),
                    CustomerNumber = HttpContext.Request.Headers.FirstOrDefault(h => h.Key == "Customer_no").Value.ToString(),
                    CustomerName = $"{HttpContext.Request.Headers.FirstOrDefault(h => h.Key == "Given_name").Value} {HttpContext.Request.Headers.FirstOrDefault(h => h.Key == "Family_name").Value}"
        };
        return Ok(obj);
    }

    [HttpGet("/public/GenerateAuthCode")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GenerateAuthCodeForTestingPurpose([FromQuery(Name = "Reference")] string reference, [FromQuery(Name = "Client")] string client)
    {
        if(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!.Equals("Test") || 
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!.Equals("Development") ||
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!.Equals("Preprod"))
        {
            var migrateUser = await _loginService.MigrateDodgeUserToAmorphie(reference);
            var user = await _userService.GetUserByReference(reference);

            var codeVerifier = "sessionRedirect";
            var codeVerifierAsByte = System.Text.Encoding.ASCII.GetBytes(codeVerifier);
            using var sha256 = SHA256.Create();
            var hashedCodeVerifier = Base64UrlEncoder.Encode(sha256.ComputeHash(codeVerifierAsByte));
            var authCodeResponse = await _authorizationService.Authorize(new AuthorizationServiceRequest{
                ClientId = client,
                CodeChallange = hashedCodeVerifier,
                CodeChallangeMethod = "SHA256",
                Nonce = "test",
                State = "test",
                User = user.Response,
                ResponseType = "code",
                Scope = ["retail-customer","openId"]
            });

            return Ok(new{
                AuthCode = authCodeResponse!.Response!.Code,
                CodeVerifier = codeVerifier
            });
        }
        return StatusCode(401);
    }

    [HttpGet("/public/open-banking-generate-auth-code")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> OpenBankingGenerateAuthCode([FromQuery(Name = "rizaNo")] Guid consentId, [FromQuery(Name = "rizaTip")] string consentType)
    {
        var consentResponse = await _consentService.GetConsent(consentId);
        
        if (consentResponse.StatusCode == 200)
        {
            var consent = consentResponse!.Response!;
            var consentData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(consent!.additionalData!);

            if(consent!.state!.Equals("K"))
            {
                return BadRequest();
            }

            string kmlkNo = consent.userTCKN!;
            var userResponse = await _userService.GetUserByReference(kmlkNo);
            if(userResponse.StatusCode != 200)
            {
                //TODO 
                //Error Handle
            }
            var user = userResponse.Response;

            var authResponse = await _authorizationService.Authorize(new AuthorizationServiceRequest()
            {
                ResponseType = "code",
                ClientId = _configuration["OpenBankingClientId"],
                Scope = ["open-banking"],
                ConsentId = consentId,
                User = user
            });
            var authCode = authResponse.Response!.Code;
            return StatusCode(201,new{
                    yetkilendirmeKodu = Guid.NewGuid().ToString(),
                    yetKod = authCode,
                    rizaNo = consentId,
                    rizaDrm =  "Y"
            });
        }
        return Forbid();
    }

    [HttpGet("/public/OpenBankingAuthCode")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> OpenBankingAuthCode(Guid consentId)
    {
        var consentResponse = await _consentService.GetConsent(consentId);
        var user = await _daprClient.GetStateAsync<LoginResponse>(_configuration["DAPR_STATE_STORE_NAME"], $"{consentId}_User");

        if (consentResponse.StatusCode == 200)
        {
            var consent = consentResponse!.Response!;
            var deserializedData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(consent.additionalData!);
            
            var redirectUri = deserializedData.gkd.yonAdr;

            var authResponse = await _authorizationService.Authorize(new AuthorizationServiceRequest()
            {
                ResponseType = "code",
                ClientId = _configuration["OpenBankingClientId"],
                Scope = ["open-banking"],
                ConsentId = consentId,
                User = user
            });
            var authCode = authResponse.Response.Code;

            return Redirect($"{redirectUri}&rizaDrm=Y&yetKod={authCode}&rizaNo={consentId}&rizaTip={OpenBankingConstants.ConsentTypeMap[consent.consentType!]}");
        }
        return Forbid();
    }

    [HttpPost("public/CreatePreLogin")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> CreatePreLogin([FromHeader(Name = "clientIdReal")] string sourceClient, [FromHeader(Name = "user_reference")] string currentUser, [FromHeader(Name = "scope")] string[] scope, [FromBody] CreatePreLoginRequest createPreLoginRequest)
    {
        var clientResponse = await _clientService.CheckClient(sourceClient);
        if (clientResponse.StatusCode != 200)
        {
            return Problem(detail: "Client not found", statusCode: 404);
        }
        var client = clientResponse.Response;

        if(!client!.CanCreateLoginUrl)
        {
            return Problem(detail: "Client is not authorized to use this flow", statusCode: 403);
        }

        ServiceResponse<ClientResponse> targetClientResponse;
        if (Guid.TryParse(createPreLoginRequest.clientCode, out Guid _))
        {
            targetClientResponse = await _clientService.CheckClient(createPreLoginRequest.clientCode);
        }
        else
        {
            targetClientResponse = await _clientService.CheckClientByCode(createPreLoginRequest.clientCode);
        }

        if (targetClientResponse.StatusCode != 200)
        {
            return Problem(detail: "Target client not found", statusCode: 404);
        }
        var targetClient = targetClientResponse.Response;


        if(!client.CreateLoginUrlClients!.Any(c => c.Equals(targetClient!.id)))
        {
            return Problem(detail: "Target client is not authorized to creating login url from given source client", statusCode: 403);
        }

        var user = await _userService.GetUserByReference(createPreLoginRequest.scopeUser);

        var preLoginGuid = Guid.NewGuid().ToString();
        var preLoginId = "Prelogin_"+preLoginGuid;
        var preLoginInfo = new PreLoginInfo
        {
            User = user.Response,
            Client = targetClient
        };

        await _daprClient.SaveStateAsync(_configuration["DAPR_STATE_STORE_NAME"], preLoginId, preLoginInfo, metadata: new Dictionary<string, string> { { "ttlInSeconds", "20" } });
        
        return Ok(new{redirectUri = _configuration["PreLoginConsumeEndPoint"]+preLoginGuid});
    }

    [HttpGet("public/ConsumePreLogin/{id}")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> ConsumePreLogin([FromRoute] string id)
    {
        var preLoginInfo = await _daprClient.GetStateAsync<PreLoginInfo>(_configuration["DAPR_STATE_STORE_NAME"], "Prelogin_"+id);
        if(preLoginInfo is not {})
            return StatusCode(403);
        
        HttpContext.Session.SetString("LoggedUser", JsonSerializer.Serialize(preLoginInfo.User));

        return Redirect(preLoginInfo.Client.loginurl!);
    }

    [HttpPost("public/DodgeCreatePreLogin")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IResult> DodgeCreatePreLogin([FromHeader(Name = "x-userinfo")] string userinfo, [FromBody] CreatePreLoginRequest createPreLoginRequest)
    {
        var userInfoModel = JsonSerializer.Deserialize<dynamic>(Convert.FromBase64String(userinfo));
        string username = string.Empty;
        
        var usernameProperty = userInfoModel!.GetProperty("username");
        username = usernameProperty.ToString();
        
        ServiceResponse<ClientResponse> targetClientResponse;
        if (Guid.TryParse(createPreLoginRequest.clientCode, out Guid _))
        {
            targetClientResponse = await _clientService.CheckClient(createPreLoginRequest.clientCode);
        }
        else
        {
            targetClientResponse = await _clientService.CheckClientByCode(createPreLoginRequest.clientCode);
        }

        if(targetClientResponse.StatusCode != 200)
        {
            return Results.Problem(detail:targetClientResponse.Detail,statusCode:targetClientResponse.StatusCode);
        }
        var targetClient = targetClientResponse.Response;

        ServiceResponse<AuthorizationResponse> authResponse;
        if(!username.Contains("ebt"))
        {
            var migrateResult = await _loginService.MigrateDodgeUserToAmorphie(username);
            if(migrateResult.StatusCode != 200)
            {
                return Results.Problem(detail:migrateResult.Detail, statusCode:migrateResult.StatusCode);
            }

            var amorphieUserResult = await _loginService.GetAmorphieUser(username);
            if(amorphieUserResult.StatusCode != 200)
            {
                return Results.Problem(detail:amorphieUserResult.Detail, statusCode:amorphieUserResult.StatusCode);
            }
            var amorphieUser = amorphieUserResult.Response;
            
            var profileResult = await _profileService.GetCustomerSimpleProfile(username);
            if(profileResult.StatusCode != 200)
            {
                return Results.Problem(detail:profileResult.Detail, statusCode:profileResult.StatusCode);
            }
            var profile = profileResult.Response;

            authResponse = await _authorizationService.Authorize(new AuthorizationServiceRequest{
                ClientId = targetClient.id,
                RedirectUri = targetClient.returnuri,
                CodeChallange = createPreLoginRequest.CodeChallange,
                CodeChallangeMethod = "SHA256",
                Nonce = createPreLoginRequest.Nonce,
                ResponseType = "code",
                State = createPreLoginRequest.State,
                Scope = ["openid","profile"],
                User = amorphieUser,
                Profile = profile
            });

            if (authResponse.StatusCode != 200)
            {
                return Results.Problem(detail:authResponse.Detail,statusCode:authResponse.StatusCode);
            }
        }
        else
        {
            var integrationUserProperty = userInfoModel!.GetProperty("integration_user_name");
            username = integrationUserProperty.ToString();
            
            var collectionUser = _collectionUsers.Users.FirstOrDefault(u => u.LoginUser.Equals(username.Split("\\")[1]));
            var amorphieUserResult = await _userService.GetUserByReference(collectionUser.CitizenshipNo);
            var amorphieUser = amorphieUserResult.Response;
            authResponse = await _authorizationService.Authorize(new AuthorizationServiceRequest
            {
                ClientId = targetClient.id,
                RedirectUri = targetClient.returnuri,
                CodeChallange = createPreLoginRequest.CodeChallange,
                CodeChallangeMethod = "SHA256",
                Nonce = createPreLoginRequest.Nonce,
                ResponseType = "code",
                State = createPreLoginRequest.State,
                Scope = ["openid","profile"],
                User = amorphieUser
            });
            if (authResponse.StatusCode != 200)
            {
                return Results.Problem(detail:authResponse.Detail,statusCode:authResponse.StatusCode);
            }

            var authCodeInfo = await _authorizationService.AssignCollectionUserToAuthorizationCode(amorphieUser!, authResponse.Response!.Code!,collectionUser);

        }

        return Results.Ok(new{AuthCode=authResponse.Response!.Code});
    }

    [HttpGet("public/OpenBankingAuthorize")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> OpenBankingAuthorize(OpenBankingAuthorizationRequest authorizationRequest)
    {
        var loginModel = new OpenBankingLogin
        {
            consentId = authorizationRequest.riza_no
        };

        var consentResult = await _consentService.GetConsent(authorizationRequest.riza_no);
        if (consentResult.StatusCode != 200)
        {
            ViewBag.ErrorDetail = consentResult.Detail;
            return View("Error");
        }
        var consent = consentResult.Response;

        if(consent!.state!.Equals("I") || consent!.state!.Equals("K") || consent!.state!.Equals("S"))
        {
            ViewBag.hasError = true;
            ViewBag.errorMessage = "Geçersiz Rıza";
            return View("NewLogin", loginModel);
        }

        if(!consent!.state!.Equals("B"))
        {
            await _consentService.CancelConsent(authorizationRequest.riza_no,"07");
            ViewBag.hasError = true;
            ViewBag.errorMessage = "Geçersiz Rıza";
            return View("NewLogin", loginModel);
        }

        var consentData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(consent!.additionalData!);
        string kmlkNo = consent.userTCKN!;

        if(!string.IsNullOrWhiteSpace(kmlkNo))
        {
            var customerInfoResult = await _profileService.GetCustomerSimpleProfile(kmlkNo);
            if (customerInfoResult.StatusCode != 200)
            {
                ViewBag.ErrorDetail = customerInfoResult.Detail;
                return View("Error");
            }
            var customerInfo = customerInfoResult.Response;
            ViewBag.isOn = customerInfo!.data!.profile!.businessLine;
        }
        else
        {
            ViewBag.isOn = "X";
        }

        if(!string.IsNullOrWhiteSpace(authorizationRequest.error_message))
        {
            ViewBag.hasError = true;
            ViewBag.errorMessage = Encoding.UTF8.GetString(Convert.FromBase64String(authorizationRequest.error_message));
        }
        else
        {
            ViewBag.hasError = false;
        }
        
        // if (customerInfo!.data!.profile!.businessLine == "X")
        // {
        //     return View("OpenBankingLoginOn", loginModel);
        // }
        // else
        // {
        //     return View("OpenBankingLoginBurgan", loginModel);
        // }
        return View("NewLogin", loginModel);

    }

    [HttpGet("public/Authorize")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> Authorize(AuthorizationRequest authorizationRequest)
    {
        // ViewBag.deviceId = Guid.NewGuid().ToString();
        // ViewBag.tokenId = Guid.NewGuid().ToString();
        // ViewBag.requestId = Guid.NewGuid().ToString();
        // ViewBag.body = JsonSerializer.Serialize(authorizationRequest);
        // HttpContext.Session.SetString("FlowUserInfo",JsonSerializer.Serialize(new FlowUserInfo(){
        //     deviceId = ViewBag.deviceId,
        //     tokenId = ViewBag.tokenId
        // }));

        // using var httpClient = new HttpClient();
        // var content = new StringContent(JsonSerializer.Serialize(new{
        //     grant_type = "client_credentials",
        //     client_id = "IbAndroidApp",
        //     client_secret = "",
        //     scopes = new string[] { "openId","retail-customer"}
        // }),Encoding.UTF8,"application/json");
        // var resp = await httpClient.PostAsync("https://test-pubagw6.burgan.com.tr/ebanking/token",content);
        // var res = await resp.Content.ReadAsStringAsync();
        
        // var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(res);
        // ViewBag.accessToken = tokenResponse!.AccessToken;

        // return View("LoginFlowPage");

        var authorize = await _authorizationService.Authorize(new AuthorizationServiceRequest
        {
            ClientId = authorizationRequest.ClientId,
            RedirectUri = authorizationRequest.RedirectUri,
            ResponseType = authorizationRequest.ResponseType,
            Scope = authorizationRequest.Scope,
            State = authorizationRequest.State
        });

        if(authorize.StatusCode != 200)
        {
            var errorModel = authorize.GetErrorDetail();
            return Problem(detail:errorModel.Detail, statusCode: errorModel.StatusCode);
        }

        var loggedUserSerialized = HttpContext.Session.GetString("LoggedUser");
        if(string.IsNullOrWhiteSpace(loggedUserSerialized))
        {
            return View("LoginPage", new Login(){Code = authorize.Response!.Code});
        }
        else
        {
            var user = JsonSerializer.Deserialize<LoginResponse>(loggedUserSerialized);
            var profileResponse = await _profileService.GetCustomerSimpleProfile(user!.Reference);
            await _authorizationService.AssignUserToAuthorizationCode(user, authorize.Response!.Code!,profileResponse.Response!);
            return Redirect(authorize.Response!.RedirectUri);
        }

    }


    [HttpGet("public/CollectionUsers")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> CollectionUsers()
    {
        await Task.CompletedTask;
        
        return Ok(_collectionUsers.Users);
    }

    [HttpGet("public/AuthorizeCollection")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> AuthorizeCollection(AuthorizationRequest authorizationRequest)
    {
        ViewBag.HasError = false;
        var authorize = await _authorizationService.Authorize(new AuthorizationServiceRequest
        {
            ClientId = authorizationRequest.ClientId,
            RedirectUri = authorizationRequest.RedirectUri,
            ResponseType = authorizationRequest.ResponseType,
            Scope = authorizationRequest.Scope,
            State = authorizationRequest.State
        });



        return View("CollectionLoginPage", new Models.Account.Login(){Code = authorize.Response.Code});

    }



}
