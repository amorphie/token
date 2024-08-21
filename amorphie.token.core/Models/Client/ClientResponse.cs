using System.Collections.Generic;
using System.Text.Json.Serialization;
using amorphie.token.core.Models.Client;

namespace amorphie.token.core.Models.Client;

public class ClientResponse
{
    public string? id { get; set; }
    public string? code { get; set; }
    public string? name { get; set; }
    public List<Flow>? flows { get; set; }
    [JsonPropertyName("allowedGrantTypes")]
    public ICollection<ClientGrantType>? allowedgranttypes { get; set; }
    [JsonPropertyName("allowedScopeTags")]
    public List<string>? allowedscopetags { get; set; }

    [JsonPropertyName("loginUrl")]
    public string? loginurl { get; set; }

    [JsonPropertyName("returnUrl")]
    public string? returnuri { get; set; }

    [JsonPropertyName("logoutUrl")]
    public string? logouturi { get; set; }

    [JsonPropertyName("clientSecret")]
    public string? clientsecret { get; set; }
    public string? pkce { get; set; }
    public Jws? jws { get; set; }
    public Idempotency? idempotency { get; set; }
    public Variant? variant { get; set; }
    public Session? session { get; set; }
    public Location? location { get; set; }
    public List<Token>? tokens { get; set; }
    [JsonPropertyName("jwtSecretSalt")]
    public string? jwtSalt { get; set; }
    public bool CanCreateLoginUrl{get;set;} = default!;
    public string[]? CreateLoginUrlClients { get; set; } = default!;
    public string? PrivateKey{get;set;}
    public string? PublicKey{get;set;}
}
