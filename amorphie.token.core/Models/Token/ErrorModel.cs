using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace amorphie.token.core.Models.Token
{
    public class ErrorModel
    {
        [JsonPropertyName("error")]
        public string? Error{get;set;}
        [JsonPropertyName("error_description")]
        public string? Description{get;set;}
    }
}