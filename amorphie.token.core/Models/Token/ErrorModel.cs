using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace amorphie.token.core.Models.Token
{
    public class ErrorModel
    {
        [JsonPropertyName("errorCode")]
        public int ErrorCode{get;set;}
        [JsonPropertyName("errorType")]
        public string? ErrorType{get;set;}
        [JsonPropertyName("error")]
        public string? Error{get;set;}
    }
}