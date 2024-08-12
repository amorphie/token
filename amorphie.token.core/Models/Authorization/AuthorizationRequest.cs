using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace amorphie.token.core.Models.Authorization;

public class AuthorizationRequest
{
    [FromQuery(Name = "response_type")]
    public string? ResponseType { get; set; }
    [FromQuery(Name = "client_id")]
    public string? ClientId { get; set; }
    [FromQuery(Name = "redirect_uri")]
    public string? RedirectUri { get; set; }
    [FromQuery(Name = "scope")]
    public string[]? Scope { get; set; }
    [FromQuery(Name = "state")]
    public string? State { get; set; }
    [FromQuery(Name = "nonce")]
    public string? Nonce { get; set; }
    [FromQuery(Name = "code_challange")]
    public string? CodeChallange { get; set; }
    [FromQuery(Name = "code_challenge_method")]
    public string? CodeChallangeMethod { get; set; }
    public string? error_message{get;set;}
}

public class AuthorizationRequestBody
{
    public string? ResponseType { get; set; }
    public string? ClientId { get; set; }
    public string? RedirectUri { get; set; }
    public string[]? Scope { get; set; }
    public string? State { get; set; }
    public string? Nonce { get; set; }
    public string? CodeChallange { get; set; }
    public string? CodeChallangeMethod { get; set; }
}
