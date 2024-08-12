using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace amorphie.token.core.Models.Consent
{
    public class YosInfo
    {
        [JsonPropertyName("kod")]
        public string Code{get;set;}
        [JsonPropertyName("unv")]
        public string Name{get;set;}
        [JsonPropertyName("acikAnahtar")]
        public string PublicKey{get;set;}
    }
}