using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace amorphie.token.core.Models.Consent
{
    public class ConsentResponse
    {
        public Guid id { get; set; }
        public Guid userId { get; set; }
        public string? state { get; set; }
        public string? consentType { get; set; }
        public string? additionalData { get; set; }
        public string? userTCKN{get;set;}
        public string? scope{get;set;}
        public DateTime? stateModifiedAt{get;set;}
        public List<OBAccountConsentDetailDto>? obAccountConsentDetails{get;set;}
        public string? userType{get;set;}
    }

 
public class OBAccountConsentDetailDto
{
    public Guid ConsentId { get; set; }
    public string IdentityType { get; set; } = string.Empty;
    public string IdentityData { get; set; } = string.Empty;
    public string? InstitutionIdentityType { get; set; }
    public string? InstitutionIdentityData { get; set; }
    public string? CustomerNumber { get; set; }
    public string? InstitutionCustomerNumber { get; set; }
    public string UserType { get; set; } = string.Empty;
    public string HhsCode { get; set; } = string.Empty;
    public string YosCode { get; set; } = string.Empty;
    public string? AuthMethod { get; set; }
    public string? ForwardingAddress { get; set; }
    public string? HhsForwardingAddress { get; set; }
    public string? DiscreteGKDDefinitionType { get; set; }
    public string? DiscreteGKDDefinitionValue { get; set; }
    public DateTime? AuthCompletionTime { get; set; }
    public List<string> PermissionTypes { get; set; } = new();
    public DateTime? TransactionInquiryStartTime { get; set; }
    public DateTime? TransactionInquiryEndTime { get; set; }
    public List<string>? AccountReferences { get; set; }
    public string? OhkMessage { get; set; }
    public string XRequestId { get; set; } = string.Empty;
    public string XGroupId { get; set; } = string.Empty;
    public int? SendToServiceTryCount { get; set; }
    public DateTime? SendToServiceLastTryTime { get; set; }
    public int? SendToServiceDeliveryStatus { get; set; }
}
 
}