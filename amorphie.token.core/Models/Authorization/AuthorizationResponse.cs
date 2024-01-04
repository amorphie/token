using System.Collections.Generic;

namespace amorphie.token.core.Models.Authorization;

public class AuthorizationResponse
{
    public string ResponseType { get; set; }
    public string Code { get; set; }
    public string State { get; set; }
    public string RedirectUri { get; set; }
    public IList<string> RequestedScopes { get; set; }
    public string GrantType { get; set; }
}
