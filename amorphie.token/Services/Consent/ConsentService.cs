using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using amorphie.token.core.Models.Consent;

namespace amorphie.token.Services.Consent
{
    public class ConsentService : ServiceBase, IConsentService
    {
        private readonly DaprClient _daprClient;
        public ConsentService(ILogger<ConsentService> logger, IConfiguration configuration, DaprClient daprClient) : base(logger, configuration)
        {
            _daprClient = daprClient;
        }

        public async Task<ServiceResponse<DocumentResponse>> CheckDocument(string clientId, string roleId, string citizenshipNo)
        {
            try
            {
                var documents = await _daprClient.InvokeMethodAsync<DocumentResponse>(HttpMethod.Post, Configuration["ConsentServiceAppName"], $"Authorization/CheckAuthorizationForLogin/clientCode={clientId}&roleId={roleId}&userTCKN={citizenshipNo}?scope={citizenshipNo}");

                return new ServiceResponse<DocumentResponse>()
                {
                    StatusCode = 200,
                    Detail = "",
                    Response = documents
                };
            }
            catch (InvocationException ex)
            {
                return new ServiceResponse<DocumentResponse>()
                {
                    StatusCode = (int)ex.Response.StatusCode,
                    Detail = await ex.Response.Content.ReadAsStringAsync()
                };
            }
            catch (System.Exception ex)
            {
                return new ServiceResponse<DocumentResponse>()
                {
                    StatusCode = 500,
                    Detail = ex.ToString()
                };
            }
        }

        public async Task<ServiceResponse> CheckConsent(string clientId, string roleId, string citizenshipNo)
        {
            try
            {
                await _daprClient.InvokeMethodAsync(HttpMethod.Get, Configuration["ConsentServiceAppName"], $"Authorization/CheckAuthorizationForLogin/clientCode={clientId}&roleId={roleId}&userTCKN={citizenshipNo}?scope={citizenshipNo}");

                return new ServiceResponse()
                {
                    StatusCode = 200,
                    Detail = ""
                };
            }
            catch (InvocationException ex)
            {
                return new ServiceResponse()
                {
                    StatusCode = (int)ex.Response.StatusCode,
                    Detail = await ex.Response.Content.ReadAsStringAsync()
                };
            }
            catch (System.Exception ex)
            {
                return new ServiceResponse()
                {
                    StatusCode = 500,
                    Detail = ex.ToString()
                };
            }
        }

        public async Task<ServiceResponse> SaveConsent(string clientId, string roleId, string citizenshipNo)
        {
            try
            {
                var request = new
                {
                    roleId = roleId,
                    clientCode = clientId,
                    userTCKN = citizenshipNo,
                    scope = citizenshipNo
                };

                await _daprClient.InvokeMethodAsync(HttpMethod.Post, Configuration["ConsentServiceAppName"], $"Authorization/AuthorizeForLogin", request);

                return new ServiceResponse()
                {
                    StatusCode = 200,
                    Detail = ""
                };
            }
            catch (InvocationException ex)
            {
                return new ServiceResponse()
                {
                    StatusCode = (int)ex.Response.StatusCode,
                    Detail = await ex.Response.Content.ReadAsStringAsync()
                };
            }
            catch (System.Exception ex)
            {
                return new ServiceResponse()
                {
                    StatusCode = 500,
                    Detail = ex.ToString()
                };
            }
        }

        public async Task<ServiceResponse<ConsentResponse>> GetConsent(Guid consentId)
        {
            try
            {
                var consent = await _daprClient.InvokeMethodAsync<ConsentResponse>(HttpMethod.Get, Configuration["ConsentServiceAppName"], "/OpenBankingConsentHHS/" + consentId.ToString());
                if(consent.obAccountConsentDetails is not null)
                {
                    consent.userType = consent.obAccountConsentDetails.FirstOrDefault()?.UserType;
                }

                return new ServiceResponse<ConsentResponse>()
                {
                    StatusCode = 200,
                    Detail = "",
                    Response = consent
                };
            }
            catch (InvocationException ex)
            {
                return new ServiceResponse<ConsentResponse>()
                {
                    StatusCode = (int)ex.Response.StatusCode,
                    Detail = await ex.Response.Content.ReadAsStringAsync()
                };
            }
            catch (System.Exception ex)
            {
                return new ServiceResponse<ConsentResponse>()
                {
                    StatusCode = 500,
                    Detail = ex.ToString()
                };
            }
        }

        public async Task<ServiceResponse> UpdateConsentForUsage(Guid consentId)
        {
            try
            {
                await _daprClient.InvokeMethodAsync<dynamic, dynamic>(Configuration["ConsentServiceAppName"], "OpenBankingConsentHHS/UpdatePaymentConsentStatusForUsage", new
                {
                    id = consentId,
                    state = "K"
                });

                return new ServiceResponse()
                {
                    StatusCode = 200,
                    Detail = ""
                };
            }
            catch (InvocationException ex)
            {
                return new ServiceResponse()
                {
                    StatusCode = (int)ex.Response.StatusCode,
                    Detail = await ex.Response.Content.ReadAsStringAsync()
                };
            }
            catch (System.Exception ex)
            {
                return new ServiceResponse()
                {
                    StatusCode = 500,
                    Detail = ex.ToString()
                };
            }

        }

        public async Task<ServiceResponse> CheckAuthorizationConsent(string clientId, string currentUser, string scopeUser)
        {
            try
            {
                var consent = await _daprClient.InvokeMethodAsync<ConsentResponse>(HttpMethod.Get, Configuration["ConsentServiceAppName"], $"/Authorization/CheckConsent/clientCode={clientId}&userTCKN={currentUser}&scope={scopeUser}");

                return new ServiceResponse()
                {
                    StatusCode = 200,
                    Detail = ""
                };
            }
            catch (InvocationException ex)
            {
                return new ServiceResponse()
                {
                    StatusCode = (int)ex.Response.StatusCode,
                    Detail = await ex.Response.Content.ReadAsStringAsync()
                };
            }
            catch (System.Exception ex)
            {
                return new ServiceResponse()
                {
                    StatusCode = 500,
                    Detail = ex.ToString()
                };
            }
        }

        public async Task<ServiceResponse> UpdateConsentInOtp(Guid consentId, string citizenshipNo)
        {
            try
            {
                await _daprClient.InvokeMethodAsync<dynamic, dynamic>(Configuration["ConsentServiceAppName"], "OpenBankingConsentHHS/UpdateConsentInOtp", new
                {
                    id = consentId,
                    userTckn = citizenshipNo
                });

                return new ServiceResponse()
                {
                    StatusCode = 200,
                    Detail = ""
                };
            }
            catch (InvocationException ex)
            {
                return new ServiceResponse()
                {
                    StatusCode = (int)ex.Response.StatusCode,
                    Detail = await ex.Response.Content.ReadAsStringAsync()
                };
            }
            catch (System.Exception ex)
            {
                return new ServiceResponse()
                {
                    StatusCode = 500,
                    Detail = ex.ToString()
                };
            }
        }

        public async Task<ServiceResponse> CheckAuthorizeForInstitutionConsent(Guid consentId, string citizenshipNo)
        {
            try
            {
                await _daprClient.InvokeMethodAsync<dynamic, dynamic>(Configuration["ConsentServiceAppName"], "OpenBankingConsentHHS/CheckAuthorizeForInstutitionConsent?consentId="+consentId+"&tckn="+citizenshipNo, new
                {
                    
                });

                return new ServiceResponse()
                {
                    StatusCode = 200,
                    Detail = ""
                };
            }
            catch (InvocationException ex)
            {
                return new ServiceResponse()
                {
                    StatusCode = (int)ex.Response.StatusCode,
                    Detail = await ex.Response.Content.ReadAsStringAsync()
                };
            }
            catch (System.Exception ex)
            {
                return new ServiceResponse()
                {
                    StatusCode = 500,
                    Detail = ex.ToString()
                };
            }
        }

        public async Task<ServiceResponse> CancelConsent(Guid consentId, string cancelDetailCode)
        {
            try
            {
                await _daprClient.InvokeMethodAsync(HttpMethod.Delete, Configuration["ConsentServiceAppName"], $"/OpenBankingConsentHHS/Cancel",new{
                    consentId = consentId,
                    cancelDetailCode = cancelDetailCode
                });

                return new ServiceResponse()
                {
                    StatusCode = 200,
                    Detail = ""
                };
            }
            catch (InvocationException ex)
            {
                return new ServiceResponse()
                {
                    StatusCode = (int)ex.Response.StatusCode,
                    Detail = await ex.Response.Content.ReadAsStringAsync()
                };
            }
            catch (System.Exception ex)
            {
                return new ServiceResponse()
                {
                    StatusCode = 500,
                    Detail = ex.ToString()
                };
            }
        }

        public async Task<ServiceResponse<YosInfo>> GetYosInfo(string code)
        {
            try
            {
                var yosInfo = await _daprClient.InvokeMethodAsync<YosInfo>(HttpMethod.Get, Configuration["ConsentServiceAppName"], "OpenBankingYosInfo/code/"+code);

                return new ServiceResponse<YosInfo>()
                {
                    StatusCode = 200,
                    Response = yosInfo,
                    Detail = ""
                };
            }
            catch (InvocationException ex)
            {
                return new ServiceResponse<YosInfo>()
                {
                    StatusCode = (int)ex.Response.StatusCode,
                    Detail = await ex.Response.Content.ReadAsStringAsync()
                };
            }
            catch (System.Exception ex)
            {
                return new ServiceResponse<YosInfo>()
                {
                    StatusCode = 500,
                    Detail = ex.ToString()
                };
            }
        }
    }
}








