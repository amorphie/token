
using System.Security.Cryptography;
using amorphie.token.core.Extensions;
using Microsoft.IdentityModel.Tokens;
using amorphie.token.data;
using amorphie.token.Services.Profile;
using amorphie.token.Services.Consent;
using amorphie.token.core.Models.Profile;
using System.Text.Json;

namespace amorphie.token.Services.Authorization;

public class AuthorizationService : ServiceBase, IAuthorizationService
{
    private readonly IClientService _clientService;
    private readonly DaprClient _daprClient;


    public AuthorizationService(ILogger<AuthorizationService> logger, IConfiguration configuration, IClientService clientService, ITagService tagService,
    IUserService userService, IConsentService consentService, DaprClient daprClient, DatabaseContext databaseContext, IProfileService profileService)
    : base(logger, configuration)
    {
        _clientService = clientService;
        _daprClient = daprClient;
    }

    public async Task<AuthorizationCode> AssignCollectionUserToAuthorizationCode(LoginResponse user, string authorizationCode,core.Models.Collection.User collectionUser)
    {
        var authorizationCodeInfo = await _daprClient.GetStateAsync<AuthorizationCode>(Configuration["DAPR_STATE_STORE_NAME"], authorizationCode);

        var newAuthorizationCodeInfo = authorizationCodeInfo.MapTo<AuthorizationCode>();
        newAuthorizationCodeInfo.Subject = user;
        newAuthorizationCodeInfo.CollectionUser = collectionUser;

        await _daprClient.SaveStateAsync(Configuration["DAPR_STATE_STORE_NAME"], authorizationCode, newAuthorizationCodeInfo);
        return await _daprClient.GetStateAsync<AuthorizationCode>(Configuration["DAPR_STATE_STORE_NAME"], authorizationCode);
    }

    public async Task<AuthorizationCode> AssignUserToAuthorizationCode(LoginResponse user, string authorizationCode,SimpleProfileResponse profile)
    {
        var authorizationCodeInfo = await _daprClient.GetStateAsync<AuthorizationCode>(Configuration["DAPR_STATE_STORE_NAME"], authorizationCode);

        var newAuthorizationCodeInfo = authorizationCodeInfo.MapTo<AuthorizationCode>();
        newAuthorizationCodeInfo.Subject = user;
        newAuthorizationCodeInfo.Profile = profile;

        await _daprClient.SaveStateAsync(Configuration["DAPR_STATE_STORE_NAME"], authorizationCode, newAuthorizationCodeInfo);
        return await _daprClient.GetStateAsync<AuthorizationCode>(Configuration["DAPR_STATE_STORE_NAME"], authorizationCode);
    }


    public async Task<ServiceResponse<AuthorizationResponse>> Authorize(AuthorizationServiceRequest request)
    {
        AuthorizationResponse authorizationResponse = new();
        try
        {
            ServiceResponse<ClientResponse>? clientResponse;
            if(Guid.TryParse(request.ClientId!,out Guid _))
            {
                clientResponse = await _clientService.CheckClient(request.ClientId!);
                if (clientResponse.StatusCode != 200)
                {
                    return new ServiceResponse<AuthorizationResponse>()
                    {
                        StatusCode = clientResponse.StatusCode,
                        Detail = clientResponse.Detail
                    };
                }
            }
            else
            {
                clientResponse = await _clientService.CheckClientByCode(request.ClientId!);
                if (clientResponse.StatusCode != 200)
                {
                    return new ServiceResponse<AuthorizationResponse>()
                    {
                        StatusCode = clientResponse.StatusCode,
                        Detail = clientResponse.Detail
                    };
                }
            }
            
            var client = clientResponse.Response;
            authorizationResponse.Client = client!;

            if (string.IsNullOrEmpty(request.ResponseType) || request.ResponseType != "code")
            {
                return new ServiceResponse<AuthorizationResponse>()
                {
                    StatusCode = 473,
                    Detail = "Response Type Parameter Has To Be Set As 'Code'"
                };
            }

            if(request.Scope.Count() == 1)
            {
                request.Scope = request.Scope[0].Split(" ");
            }
            var requestedScopes = request.Scope!.ToList();
            

            if (!requestedScopes.All(client!.allowedscopetags!.Contains))
            {
                return new ServiceResponse<AuthorizationResponse>()
                {
                    StatusCode = 473,
                    Detail = "Client is Not Authorized For Requested Scopes"
                };
            }

            if(string.IsNullOrWhiteSpace(client!.returnuri))
            {
                return new ServiceResponse<AuthorizationResponse>()
                {
                    StatusCode = 480,
                    Detail = "Client Redirect Uri is Not Defined"
                };
            }

            if(!client!.returnuri!.Equals(request.RedirectUri))
            {
                return new ServiceResponse<AuthorizationResponse>()
                {
                    StatusCode = 481,
                    Detail = "Requested Redirect Uri Doesn't Match With Defined on Client"
                };
            }

            if(!string.IsNullOrWhiteSpace(client!.pkce))
            {
                if(client.pkce.Equals("must"))
                {
                    if(string.IsNullOrWhiteSpace(request.CodeChallangeMethod))
                    {
                        return new ServiceResponse<AuthorizationResponse>()
                        {
                            StatusCode = 482,
                            Detail = "code_challange_method Parameter is Mandatory"
                        };
                    }
                    
                    if(!TokenConstants.SupportedPkceAlgs.Contains(request.CodeChallangeMethod))
                    {
                        return new ServiceResponse<AuthorizationResponse>()
                        {
                            StatusCode = 483,
                            Detail = $"code_challange_method has to be set as one of following ({String.Join(",",TokenConstants.SupportedPkceAlgs)})"
                        };
                    }

                    if(string.IsNullOrWhiteSpace(request.CodeChallange))
                    {
                        return new ServiceResponse<AuthorizationResponse>()
                        {
                            StatusCode = 484,
                            Detail = "code_challange Parameter is Mandatory"
                        };
                    }
                    
                }

            }

            var authCode = new AuthorizationCode
            {
                ClientId = client.id,
                RedirectUri = request.RedirectUri,
                RequestedScopes = requestedScopes,
                CodeChallenge = request.CodeChallange,
                CodeChallengeMethod = request.CodeChallangeMethod,
                CreationTime = DateTime.UtcNow,
                Subject = request.User ?? null,
                ConsentId = request.ConsentId?.ToString(),
                Nonce = request.Nonce,
                State = request.State,
                Profile = request.Profile
            };
            
            var code = await GenerateAuthorizationCode(authCode, request.ClientId!.Equals(Configuration["OpenBankingClientId"]) ? "300" : "60");
            
            if(string.IsNullOrWhiteSpace(request.State))
            {
                authorizationResponse.RedirectUri = $"{client.returnuri}?response_type=code&code={code}";
            }
            else
            {
                authorizationResponse.RedirectUri = $"{client.returnuri}?response_type=code&code={code}&state={request.State}";
            }
            authorizationResponse.Code = code;
            authorizationResponse.RequestedScopes = requestedScopes;
            authorizationResponse.State = request.State!;

            return new ServiceResponse<AuthorizationResponse>()
            {
                StatusCode = 200,
                Response = authorizationResponse
            };

        }
        catch (Exception ex)
        {
            Logger.LogError(ex.Message);
            return new ServiceResponse<AuthorizationResponse>()
            {
                StatusCode = 500,
                Response = null
            };
        }

    }

    private async Task<string> GenerateAuthorizationCode(AuthorizationCode authorizationCode, string ttl)
    {
        var rand = RandomNumberGenerator.Create();
        byte[] bytes = new byte[32];
        rand.GetBytes(bytes);
        var code = Base64UrlEncoder.Encode(bytes);

        await _daprClient.SaveStateAsync(Configuration["DAPR_STATE_STORE_NAME"], code, authorizationCode, metadata: new Dictionary<string, string> { { "ttlInSeconds", ttl } });

        return code;
    }

}
