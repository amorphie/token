using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using amorphie.token.core.Models.Consent;
using amorphie.token.core.Models.InternetBanking;
using amorphie.token.core.Models.Profile;
using amorphie.token.data;
using amorphie.token.Services.ClaimHandler;
using amorphie.token.Services.Consent;
using amorphie.token.Services.InternetBanking;
using amorphie.token.Services.Profile;
using amorphie.token.Services.Role;
using amorphie.token.Services.TransactionHandler;

namespace amorphie.token.Services.Token
{
    public class TokenServiceV2(ILogger<AuthorizationService> logger, IConfiguration configuration, IClientService clientService, IClaimHandlerService claimService,
ITransactionService transactionService, IRoleService roleService, IbDatabaseContextMordor ibContextMordor, IConsentService consentService, IUserService userService, DaprClient daprClient, DatabaseContext databaseContext, IbSecurityDatabaseContext securityContext
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

        public string DeviceId { set => throw new NotImplementedException(); }

        public Task<ServiceResponse<TokenResponse>> GenerateOpenBankingToken(GenerateTokenRequest tokenRequest, ConsentResponse consent)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<TokenResponse>> GenerateToken(GenerateTokenRequest tokenRequest)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<TokenResponse>> GenerateTokenWithClientCredentials(GenerateTokenRequest tokenRequest)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<TokenResponse>> GenerateTokenWithDevice(GenerateTokenRequest tokenRequest)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<TokenResponse>> GenerateTokenWithPassword(GenerateTokenRequest tokenRequest)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<TokenResponse>> GenerateTokenWithPasswordFromWorkflow(GenerateTokenRequest tokenRequest, ClientResponse client, LoginResponse user, SimpleProfileResponse? profile, string deviceId)
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResponse<TokenResponse>> GenerateTokenWithRefreshToken(GenerateTokenRequest tokenRequest)
        {

            ServiceResponse<ClientResponse> clientResponse = await GetClient(tokenRequest.ClientId!,tokenRequest.ClientSecret);
            if (clientResponse.StatusCode != 200)
            {
                return new ServiceResponse<TokenResponse>()
                {
                    StatusCode = clientResponse.StatusCode,
                    Detail = clientResponse.Detail
                };
            }
            var client = clientResponse.Response;

            if (!client!.allowedgranttypes!.Any(g => g.GrantType == tokenRequest.GrantType))
            {
                return new ServiceResponse<TokenResponse>()
                {
                    StatusCode = 471,
                    Detail = "Client Has No Authorize To Use Requested Grant Type"
                };
            }

            var requestedScopes = tokenRequest.Scopes!.ToList();

            if (!requestedScopes.All(client.allowedscopetags!.Contains))
            {
                return new ServiceResponse<TokenResponse>()
                {
                    StatusCode = 473,
                    Detail = "Client is Not Authorized For Requested Scopes"
                };
            }
        
            //Decide How To Check Username and Password - Workflow 
            

        
            // var tokenResponse = await GenerateTokenResponse();

            // if (tokenResponse.IdToken == string.Empty && tokenResponse.AccessToken == string.Empty)
            // {
            //     return new ServiceResponse<TokenResponse>()
            //     {
            //         StatusCode = 500,
            //         Detail = "Access Token And Id Token Couldn't Be Generated"
            //     };
            // }

            // await PersistTokenInfo();

            return new ServiceResponse<TokenResponse>()
            {
                StatusCode = 200,
                Response = null
            };
        }

        public Task<ServiceResponse<TokenResponse>> GenerateTokenWithRefreshTokenFromWorkflowAsync(TokenInfo refreshTokenInfo, ClientResponse client, LoginResponse user, SimpleProfileResponse? profile, IBUser? dodgeUser, int? dodgeRoleKey, ConsentResponse? consent)
        {
            throw new NotImplementedException();
        }

        private async Task<ServiceResponse<ClientResponse>> GetClient(string clientCode, string? clientSecret = null)
        {
            ServiceResponse<ClientResponse> clientResponse;
            if(string.IsNullOrEmpty(clientSecret))
            {
                if (Guid.TryParse(clientCode, out Guid _))
                {
                    clientResponse = await _clientService.CheckClient(clientCode);
                }
                else
                {
                    clientResponse = await _clientService.CheckClientByCode(clientCode);
                }
            }
            else
            {
                if (Guid.TryParse(clientCode, out Guid _))
                {
                    clientResponse = await _clientService.ValidateClient(clientCode, clientSecret!);
                }
                else
                {
                    clientResponse = await _clientService.ValidateClientByCode(clientCode, clientSecret!);
                }
            }

            return clientResponse;
        }
    }
}