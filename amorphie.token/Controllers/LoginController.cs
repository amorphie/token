using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using amorphie.token.data;
using Login = amorphie.token.core.Models.Account.Login;
using amorphie.token.Services.InternetBanking;
using amorphie.token.Services.Profile;
using amorphie.token.Services.FlowHandler;
using amorphie.token.Services.Consent;
using System.Dynamic;
using amorphie.token.Services.TransactionHandler;
using amorphie.core.Enums;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace amorphie.token.core.Controllers;


public class LoginController : Controller
{
    private readonly ILogger<TokenController> _logger;
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
    private readonly IbDatabaseContext _ibContext;
    private readonly IInternetBankingUserService _internetBankingUserService;
    private readonly CollectionUsers _collectionUsers;
    public LoginController(ILogger<TokenController> logger, IAuthorizationService authorizationService, IUserService userService, DatabaseContext databaseContext
    , IConfiguration configuration,IInternetBankingUserService internetBankingUserService, DaprClient daprClient, IClientService clientService, IInternetBankingUserService ibUserService, ITransactionService transactionService,
    IFlowHandler flowHandler, CollectionUsers collectionUsers, IConsentService consentService, IProfileService profileService, IbDatabaseContext ibContext)
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
        _ibContext = ibContext;
        _internetBankingUserService = internetBankingUserService;
        _collectionUsers = collectionUsers;
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost("ebanking/Authorize/login")]
    public async Task<IActionResult> Login(Login loginRequest)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(loginRequest.UserName) || string.IsNullOrWhiteSpace(loginRequest.Password))
            {
                ViewBag.HasError = true;
                ViewBag.ErrorDetail = "Reference and Password Can Not Be Empty";
            }

            var userResponse = await _internetBankingUserService.GetUser(loginRequest.UserName!);
            if (userResponse.StatusCode != 200)
            {
                
            }
            var user = userResponse.Response;

            var passwordResponse = await _internetBankingUserService.GetPassword(user!.Id);
            if (passwordResponse.StatusCode != 200)
            {
                
            }
            var passwordRecord = passwordResponse.Response;

            var isVerified = _internetBankingUserService.VerifyPassword(passwordRecord!.HashedPassword!, loginRequest.Password!, passwordRecord.Id.ToString());
            //Consider SuccessRehashNeeded
            if (isVerified != PasswordVerificationResult.Success)
            {
                ViewBag.HasError = true;
                ViewBag.ErrorDetail = "User Is Disabled";
                var loginModel = new Login()
                {
                    Code = loginRequest.Code,
                    RedirectUri = loginRequest.RedirectUri,
                    RequestedScopes = loginRequest.RequestedScopes
                };
                return View("Authorize/Login", loginModel);
            } 
            else
            {
                var profileResponse = await _profileService.GetCustomerSimpleProfile(user.UserName);
                if (profileResponse.StatusCode != 200)
                {
                    
                }

                var userInfo = profileResponse.Response;

                var mobilePhone = userInfo!.data!.phones!.FirstOrDefault(p => p.type!.Equals("mobile"));

                var userRequest = new UserInfo
                {
                firstName = userInfo!.data.profile!.name!,
                lastName = userInfo!.data.profile!.surname!,
                phone = new core.Models.User.UserPhone()
                {
                    countryCode = mobilePhone!.countryCode!,
                    prefix = mobilePhone!.prefix,
                    number = mobilePhone!.number
                },
                state = "Active",
                salt = passwordRecord.Id.ToString(),
                password = loginRequest.Password!,
                explanation = "Migrated From IB",
                reason = "Amorphie Login",
                isArgonHash = true
                };

                var verifiedMailAddress = userInfo.data.emails!.FirstOrDefault(m => m.isVerified == true);
                userRequest.eMail = verifiedMailAddress?.address ?? "";
                userRequest.reference = loginRequest.UserName!;

                var migrateResult = await _userService.SaveUser(userRequest);
                var amorphieUserResult = await _userService.Login(new LoginRequest() { Reference = loginRequest.UserName!, Password = loginRequest.Password! });
                var amorphieUser = amorphieUserResult.Response;

                HttpContext.Session.SetString("LoggedUser", JsonSerializer.Serialize(amorphieUser));
                
                var authCodeInfo = await _authorizationService.AssignUserToAuthorizationCode(amorphieUser, loginRequest.Code!, profileResponse.Response!);

                return Redirect($"{authCodeInfo.RedirectUri}?code={loginRequest.Code}&response_type=code&state={authCodeInfo.State}");
           
                
                
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Login Failed! Ex:{0}",ex.ToString());
            return StatusCode(500);
        }
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost("public/CollectionLogin")]
    public async Task<IActionResult> CollectionLogin(Login loginRequest)
    {
        try
        {
            ViewBag.HasError = false;
            if (string.IsNullOrWhiteSpace(loginRequest.UserName) || string.IsNullOrWhiteSpace(loginRequest.Password))
            {
                ViewBag.HasError = true;
                ViewBag.ErrorDetail = "Kimlik numarası ve şifre boş olamaz";
                var loginModel = new Login()
                {
                    Code = loginRequest.Code,
                    RedirectUri = loginRequest.RedirectUri,
                    RequestedScopes = loginRequest.RequestedScopes
                };
                return View("CollectionLoginPage", loginModel);
            }

            var amorphieUserResult = await _userService.Login(new LoginRequest() { Reference = loginRequest.UserName!, Password = loginRequest.Password! });
                
            if(amorphieUserResult.StatusCode != 200)
            {
                ViewBag.HasError = true;
                ViewBag.ErrorDetail = "Hatalı şifre";
                var loginModel = new Login()
                {
                    Code = loginRequest.Code,
                    RedirectUri = loginRequest.RedirectUri,
                    RequestedScopes = loginRequest.RequestedScopes
                };
                
                return View("/Views/Authorize/CollectionLoginPage.cshtml",loginModel);
            }
            else
            {
                var amorphieUser = amorphieUserResult.Response;
                HttpContext.Session.SetString("LoggedUser", JsonSerializer.Serialize(amorphieUser));
                var collectionUser = _collectionUsers.Users.FirstOrDefault(u => u.CitizenshipNo.Equals(loginRequest.UserName));
                var authCodeInfo = await _authorizationService.AssignCollectionUserToAuthorizationCode(amorphieUser!, loginRequest.Code!,collectionUser);
                
                return Redirect($"{authCodeInfo.RedirectUri}?code={loginRequest.Code}&response_type=code&state={authCodeInfo.State}");
           
            }

            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            return StatusCode(500);
        }
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpGet("public/CheckDevice/{reference}")]
    public async Task<IActionResult> CheckDevice(string reference)
    {

        var userResponse = await _ibUserService.GetUser(reference);
        if (userResponse.StatusCode != 200)
        {
            return StatusCode(404);
        }
        var user = userResponse.Response;
        var device = await _ibContext.UserDevice.FirstOrDefaultAsync(u => u.UserId == user!.Id && u.Status == 10 && !string.IsNullOrWhiteSpace(u.DeviceToken));

        if (device != null)
        {
            return Ok(new { os = device.Platform.ToLower().Equals("android") ? 1 : 2 });
        }
        else
        {
            return StatusCode(404);
        }
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost("public/OpenBankingLogin")]
    public async Task<IActionResult> OpenBankingLogin(OpenBankingLogin openBankingLoginRequest)
    {
        try
        {
            var consentResponse = await _consentService.GetConsent(openBankingLoginRequest.consentId);
            if(consentResponse.StatusCode != 200)
            {
                await _consentService.CancelConsent(openBankingLoginRequest.consentId,"99");
                return RedirectToAction("OpenBankingAuthorize","Authorize",new
                {
                    riza_no=openBankingLoginRequest.consentId,
                    error_message = Convert.ToBase64String(Encoding.UTF8.GetBytes("Beklenmeyen bir hata oluştu."))
                });
            }
            var consent = consentResponse.Response;
            if(!string.IsNullOrWhiteSpace(consent!.userTCKN))
            {
                if(!consent!.userTCKN!.Equals(openBankingLoginRequest.username))
                {
                    await _consentService.CancelConsent(openBankingLoginRequest.consentId, "08");
                    return RedirectToAction("OpenBankingAuthorize","Authorize",new
                    {
                        riza_no=openBankingLoginRequest.consentId,
                        error_message = Convert.ToBase64String(Encoding.UTF8.GetBytes("Giriş yapmak isteyen kullanıcı ile rıza sahibi kullanıcı aynı olmalıdır."))
                    });
                }
            }
            var consentData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(consent!.additionalData!);
            var ohkTur = string.Empty;
            if (consent.consentType.Equals("OB_Account"))
            {
                ohkTur = consentData!.kmlk.ohkTur.ToString();
            }
            if (consent.consentType.Equals("OB_Payment"))
            {
                try
                {
                    ohkTur = consentData!.odmBsltm.kmlk.ohkTur.ToString();
                }
                catch (Exception ex)
                {
                    ohkTur = "";
                }
                
            }
            if(ohkTur.Equals("K"))
            {
                var checkAuthorize = await _consentService.CheckAuthorizeForInstitutionConsent(openBankingLoginRequest.consentId, openBankingLoginRequest.username);
                if(checkAuthorize.StatusCode != 200)
                {
                    //await _consentService.CancelConsent(openBankingLoginRequest.consentId,"10");
                    return RedirectToAction("OpenBankingAuthorize","Authorize",new
                    {
                        riza_no=openBankingLoginRequest.consentId,
                        error_message = Convert.ToBase64String(Encoding.UTF8.GetBytes("Kurumsal açık bankacılık işlemine izin vermelisiniz."))
                    });
                }
            }

            var userResponse = await _ibUserService.GetUser(openBankingLoginRequest.username!);
            if (userResponse.StatusCode != 200)
            {
                //TODO
                return StatusCode(500);
            }

            var passwordResponse = await _ibUserService.GetPassword(userResponse.Response!.Id);
            if (passwordResponse.StatusCode != 200)
            {
                //TODO
                return StatusCode(500);
            }
            var passwordRecord = passwordResponse.Response;

            var isVerified = _ibUserService.VerifyPassword(passwordRecord!.HashedPassword!, openBankingLoginRequest.password!, passwordRecord.Id.ToString());
            //Consider SuccessRehashNeeded
            if (isVerified != PasswordVerificationResult.Success)
            {
                passwordRecord.AccessFailedCount = (passwordRecord.AccessFailedCount ?? 0) + 1;
                //TODO - Disable User After Several Failed Login Attemps
                await _ibContext.SaveChangesAsync();
                return StatusCode(500);
            }
            else
            {
                passwordRecord.AccessFailedCount = 0;
                await _ibContext.SaveChangesAsync();
            }

            var userInfoResult = await _profileService.GetCustomerSimpleProfile(openBankingLoginRequest.username!);
            if (userInfoResult.StatusCode != 200)
            {
                //TODO
                return StatusCode(500);
            }

            var userStatus = await _ibContext.Status.Where(s => s.UserId == userResponse.Response!.Id && (!s.State.HasValue || s.State.Value == 10)).OrderByDescending(s => s.CreatedAt).FirstOrDefaultAsync();
            if (userStatus?.Type != 20)
            {
                await _consentService.CancelConsent(openBankingLoginRequest.consentId,"12");
                return RedirectToAction("OpenBankingAuthorize","Authorize",new
                {
                    riza_no=openBankingLoginRequest.consentId,
                    error_message = Convert.ToBase64String(Encoding.UTF8.GetBytes("Giriş yapmak için yetkiniz yoktur."))
                });
            }

            var userInfo = userInfoResult.Response;
            
            if (userInfo!.data!.profile!.Equals("customer") || !userInfo!.data!.profile!.status!.Equals("active"))
            {
                await _consentService.CancelConsent(openBankingLoginRequest.consentId,"12");
                return RedirectToAction("OpenBankingAuthorize","Authorize",new
                {
                    riza_no=openBankingLoginRequest.consentId,
                    error_message = Convert.ToBase64String(Encoding.UTF8.GetBytes("Giriş yapmak için yetkiniz yoktur."))
                });
            }

            var mobilePhoneCount = userInfo!.data!.phones!.Count(p => p.type!.Equals("mobile"));
            if (mobilePhoneCount != 1)
            {
                await _consentService.CancelConsent(openBankingLoginRequest.consentId,"99");
                return RedirectToAction("OpenBankingAuthorize","Authorize",new
                {
                    riza_no=openBankingLoginRequest.consentId,
                    error_message = Convert.ToBase64String(Encoding.UTF8.GetBytes("Beklenmeyen bir hata oluştu."))
                });
            }

            var mobilePhone = userInfo!.data!.phones!.FirstOrDefault(p => p.type!.Equals("mobile"));
            if (string.IsNullOrWhiteSpace(mobilePhone!.prefix) || string.IsNullOrWhiteSpace(mobilePhone!.number))
            {
                await _consentService.CancelConsent(openBankingLoginRequest.consentId,"99");
                return RedirectToAction("OpenBankingAuthorize","Authorize",new
                {
                    riza_no=openBankingLoginRequest.consentId,
                    error_message = Convert.ToBase64String(Encoding.UTF8.GetBytes("Beklenmeyen bir hata oluştu."))
                });
            }

            int roleKey = 0;
            var dodgeUserResponse = await _internetBankingUserService.GetUser(openBankingLoginRequest.username!);
            if (dodgeUserResponse.StatusCode != 200)
            {
                roleKey = 20;
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
                        roleKey = roleDefinition.Key;
                    }
                }
            }

            if(roleKey != 10)
            {
                await _consentService.CancelConsent(openBankingLoginRequest.consentId,"11");
                return RedirectToAction("OpenBankingAuthorize","Authorize",new
                {
                    riza_no=openBankingLoginRequest.consentId,
                    error_message = Convert.ToBase64String(Encoding.UTF8.GetBytes("Giriş yapmak için yetkiniz yoktur."))
                });
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
                password = openBankingLoginRequest.password!,
                explanation = "Migrated From IB",
                reason = "Amorphie Login",
                isArgonHash = true
            };

            var verifiedMailAddress = userInfo.data.emails!.FirstOrDefault(m => m.isVerified == true);
            userRequest.eMail = verifiedMailAddress?.address ?? "";
            userRequest.reference = openBankingLoginRequest.username!;

            var migrateResult = await _userService.SaveUser(userRequest);

            var amorphieUserResult = await _userService.Login(new LoginRequest() { Reference = openBankingLoginRequest.username!, Password = openBankingLoginRequest.password! });
            var amorphieUser = amorphieUserResult.Response;

            var rand = new Random();
            var code = String.Empty;
            
            for (int i = 0; i < 6; i++)
            {
                code += rand.Next(10);
            }

            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (env != null && !env.Equals("Prod"))
                code = "123456";

            var transactionId = Guid.NewGuid();
            await _daprClient.SaveStateAsync(_configuration["DAPR_STATE_STORE_NAME"], $"{transactionId}_Login_Otp_Code", code);

            await _daprClient.SaveStateAsync(_configuration["DAPR_STATE_STORE_NAME"], $"{openBankingLoginRequest.consentId}_User", amorphieUser);

            var otpRequest = new
            {
                Sender = "AutoDetect",
                SmsType = "Otp",
                Phone = new
                {
                    amorphieUser!.MobilePhone!.CountryCode,
                    amorphieUser!.MobilePhone.Prefix,
                    amorphieUser!.MobilePhone.Number
                },
                Content = $"{code} şifresi ile giriş yapabilirsiniz",
                Process = new
                {
                    Name = "Token Login Flow",
                    Identity = "Otp Login"
                }
            };

            StringContent request = new(JsonSerializer.Serialize(otpRequest), Encoding.UTF8, "application/json");

            using var httpClient = new HttpClient();
            var httpResponse = await httpClient.PostAsync(_configuration["MessagingGatewayUri"], request);

            if (!httpResponse.IsSuccessStatusCode)
            {
                //TODO
            }



            return View("newOtp", new Otp
            {
                OtpValue = (env != null && !env.Equals("Prod")) ? "123456" : "",
                Phone = "0" + amorphieUser.MobilePhone.Prefix.ToString().Substring(0, 2) + "******" + amorphieUser.MobilePhone.Number.ToString().Substring(amorphieUser.MobilePhone.Number.Length - 2, 2),
                transactionId = transactionId,
                consentId = openBankingLoginRequest.consentId,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            return RedirectToAction("OpenBankingAuthorize","Authorize",new
            {
                riza_no=openBankingLoginRequest.consentId,
                error_message = Convert.ToBase64String(Encoding.UTF8.GetBytes("Beklenmeyen bir hata oluştu."))
            });
        }
    }

    [HttpPost("public/ValidateOtp")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> ValidateOtp(Otp otpRequest)
    {
        var consentResult = await _consentService.GetConsent(otpRequest.consentId);
        if (consentResult.StatusCode != 200)
        {
            ViewBag.ErrorDetail = consentResult.Detail;
            return View("Error");
        }
        var consent = consentResult.Response;

        var sendedOtpValue = await _daprClient.GetStateAsync<string>(_configuration["DAPR_STATE_STORE_NAME"], $"{otpRequest.transactionId}_Login_Otp_Code");
        if (sendedOtpValue.Equals(otpRequest.OtpValue))
        {
            if(string.IsNullOrWhiteSpace(consent!.userTCKN))
            {
                var user = await _daprClient.GetStateAsync<LoginResponse>(_configuration["DAPR_STATE_STORE_NAME"], $"{consent.id}_User");

                await _consentService.UpdateConsentInOtp(consent.id,user.Reference);
            }

            if (consent!.consentType!.Equals("OB_Account"))
            {
                return Redirect(_configuration["OpenBankingAccount"] + otpRequest.consentId);
            }
            if (consent!.consentType!.Equals("OB_Payment"))
            {
                return Redirect(_configuration["OpenBankingPayment"] + otpRequest.consentId);
            }
            return Forbid();
        }
        else
        {
            return Forbid();
        }
    }


}
