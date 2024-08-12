using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using amorphie.token.core.Models.Consent;

namespace amorphie.token.Services.Consent
{
    public interface IConsentService
    {
        public Task<ServiceResponse<ConsentResponse>> GetConsent(Guid consentId);
        public Task<ServiceResponse> CancelConsent(Guid consentId, string cancelDetailCode);
        public Task<ServiceResponse> UpdateConsentForUsage(Guid consentId);
        public Task<ServiceResponse> UpdateConsentInOtp(Guid consentId,string citizenshipNo);
        public Task<ServiceResponse> CheckAuthorizeForInstitutionConsent(Guid consentId,string citizenshipNo);
        public Task<ServiceResponse<DocumentResponse>> CheckDocument(string clientId, string roleId, string citizenshipNo);
        public Task<ServiceResponse> CheckConsent(string clientId, string roleId, string citizenshipNo);
        public Task<ServiceResponse> SaveConsent(string clientId, string roleId, string citizenshipNo);
        public Task<ServiceResponse> CheckAuthorizationConsent(string clientId, string currentUser, string scopeUser);
        public Task<ServiceResponse<YosInfo>> GetYosInfo(string code);
    }
}