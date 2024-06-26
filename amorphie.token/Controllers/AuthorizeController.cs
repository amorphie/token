﻿using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using amorphie.token.data;
using amorphie.token.Services.InternetBanking;
using amorphie.token.Services.Profile;
using amorphie.token.Services.FlowHandler;
using amorphie.token.Services.Consent;
using amorphie.token.Services.TransactionHandler;
using amorphie.token.core.Models.Workflow;
using amorphie.token.core.Constants;
using Microsoft.Identity.Client;

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
    public AuthorizeController(ILogger<AuthorizeController> logger, IAuthorizationService authorizationService, IUserService userService, DatabaseContext databaseContext
    , IConfiguration configuration, DaprClient daprClient, IClientService clientService, IInternetBankingUserService ibUserService, ITransactionService transactionService,
    IFlowHandler flowHandler, IConsentService consentService, IProfileService profileService)
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

            return Redirect($"{redirectUri}&rizaDrm=Y&yetKod={authCode}&rizaNo={consentId}&rizaTip={OpenBankingConstants.ConsentTypeMap[consent.consentType]}");
        }
        return Forbid();
    }

    [HttpPost("public/CreatePreLoginDemo")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> PreLoginDemo([FromHeader(Name = "Authorization")] string token)
    {
        var access_token = token.Split(" ")[1];
        var tokenJti = JwtHelper.GetClaim(access_token,"jti");
        var tokenInfo = _databaseContext.Tokens.FirstOrDefault(t => t.Id.Equals(Guid.Parse(tokenJti)));
        CreatePreLoginRequest req = new CreatePreLoginRequest();
        req.clientCode = "4fa85f64-5711-4562-b3fc-2c963f66afa6";
        req.CodeChallange = "123";
        req.Nonce = "123";
        req.State = "123";
        req.scopeUser = "39719021136";
        
        using var httpClient = new HttpClient();
        StringContent request = new(JsonSerializer.Serialize(req), Encoding.UTF8, "application/json");
        request.Headers.Add("clientIdReal","3fa85f64-5717-4562-b3fc-2c963f66afa6");
        request.Headers.Add("scope","retail-customer");
        var httpResponse = await httpClient.PostAsync(_configuration["localAddress"] + "public/CreatePreLogin", request);        
        return Ok(httpResponse.Content);
    }

    [HttpPost("public/CreatePreLogin")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> CreatePreLogin([FromHeader(Name = "clientIdReal")] string sourceClient, [FromHeader(Name = "user_reference")] string currentUser, [FromHeader(Name = "scope")] string[] scope,[FromBody] CreatePreLoginRequest createPreLoginRequest)
    {
        var clientResponse = await _clientService.CheckClient(sourceClient);
        if(clientResponse.StatusCode != 200)
        {
            return Problem(detail:"Client not found",statusCode:404);
        }
        var client = clientResponse.Response;
        if(!client.CanCreateLoginUrl)
        {
            return Problem(detail:"Client is not authorized to use this flow",statusCode:403);
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

        if(targetClientResponse.StatusCode != 200)
        {
            return Problem(detail:"Target client not found",statusCode:404);
        }
        var targetClient = targetClientResponse.Response;

        if(!client.CreateLoginUrlClients.Any(c => c.Equals(targetClient.id)))
        {
            return Problem(detail:"Target client is not authorized to creating login url from given source client",statusCode:403);
        }

        var user = await _userService.GetUserByReference(createPreLoginRequest.scopeUser);
        //TODO Consent Check
        var consentResponse = await _consentService.CheckAuthorizationConsent(targetClient.id,currentUser,createPreLoginRequest.scopeUser);
        if(consentResponse.StatusCode != 200)
        {
            return Problem(detail:"User has no consent for this operation, provide a valid consent first",statusCode:403);
        }

        var authResponse = await _authorizationService.Authorize(new AuthorizationServiceRequest{
            ClientId = targetClient.id,
            RedirectUri = targetClient.returnuri,
            CodeChallange = createPreLoginRequest.CodeChallange,
            CodeChallangeMethod = "SHA256",
            Nonce = createPreLoginRequest.Nonce,
            ResponseType = "code",
            State = createPreLoginRequest.State,
            Scope = scope,
            User = user.Response
        });

        if(authResponse.StatusCode != 200)
        {
            return Problem(detail:authResponse.Detail,statusCode:authResponse.StatusCode);
        }

        //return Ok(authResponse.Response);
        return Redirect(authResponse.Response.RedirectUri);
    }

    [HttpGet("public/OpenBankingAuthorize")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> OpenBankingAuthorize(OpenBankingAuthorizationRequest authorizationRequest)
    {
        var consentResult = await _consentService.GetConsent(authorizationRequest.riza_no);
        if (consentResult.StatusCode != 200)
        {
            ViewBag.ErrorDetail = consentResult.Detail;
            return View("Error");
        }
        var consent = consentResult.Response;

        var consentData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(consent!.additionalData!);
        string kmlkNo = string.Empty;
        if (consent.consentType.Equals("OB_Account"))
        {
            kmlkNo = consentData!.kmlk.kmlkVrs.ToString();
        }
        if (consent.consentType.Equals("OB_Payment"))
        {
            kmlkNo = consentData!.odmBsltm.kmlk.kmlkVrs.ToString();
        }

        var customerInfoResult = await _profileService.GetCustomerSimpleProfile(kmlkNo);
        if (customerInfoResult.StatusCode != 200)
        {
            ViewBag.ErrorDetail = customerInfoResult.Detail;
            return View("Error");
        }
        var customerInfo = customerInfoResult.Response;

        var loginModel = new OpenBankingLogin
        {
            consentId = authorizationRequest.riza_no
        };

        if (customerInfo!.data!.profile!.businessLine == "X")
        {
            return View("OpenBankingLoginOn", loginModel);
        }
        else
        {
            return View("OpenBankingLoginBurgan", loginModel);
        }

    }

    [HttpGet("public/Authorize")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> Authorize(AuthorizationRequest authorizationRequest)
    {
        var authorize = await _authorizationService.Authorize(new AuthorizationServiceRequest
        {
            ClientId = authorizationRequest.ClientId,
            RedirectUri = authorizationRequest.RedirectUri,
            ResponseType = authorizationRequest.ResponseType,
            Scope = authorizationRequest.Scope,
            State = authorizationRequest.State
        });


        return View("LoginPage", new Models.Account.Login(){Code = authorize.Response.Code});

    }



}
