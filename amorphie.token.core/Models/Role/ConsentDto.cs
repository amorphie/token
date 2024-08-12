using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace amorphie.token.core.Models.Role
{
    public class ConsentDto
    {
        [JsonPropertyName("id")]
        public Guid Id{get;set;}
        [JsonPropertyName("clientCode")]
        public string ClientCode{get;set;}
        [JsonPropertyName("roleId")]
        public Guid RoleId{get;set;}
        [JsonPropertyName("userTCKN")]
        public long UserReference{get;set;}
        [JsonPropertyName("scope")]
        public string ScopeReference{get;set;}
    }
}