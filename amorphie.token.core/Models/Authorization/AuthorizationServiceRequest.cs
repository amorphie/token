using amorphie.token.core.Models.Profile;
using amorphie.token.core.Models.User;
using Newtonsoft.Json;

namespace amorphie.token.core.Models.Authorization;

public class AuthorizationServiceRequest
{
    public string? ResponseType { get; set; }
    public string? ClientId { get; set; }
    public string? RedirectUri { get; set; }
    public string[]? Scope { get; set; }
    public string? State { get; set; }
    public string? Nonce { get; set; }
    public string? CodeChallange { get; set; }
    public string? CodeChallangeMethod { get; set; }
    public Guid? ConsentId { get; set; }
    public LoginResponse? User { get; set; } = null;
    public SimpleProfileResponse? Profile { get; set; } = null;
}
