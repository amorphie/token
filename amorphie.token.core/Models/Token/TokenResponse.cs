using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace amorphie.token.core.Models.Token;

public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }
    [JsonPropertyName("id_token")]
    public string? IdToken { get; set; }
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }
    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
    [JsonPropertyName("refresh_token_expires_in")]
    public int RefreshTokenExpiresIn{get;set;}
}
