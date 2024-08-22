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
        public InnerErrorModel Error{get;set;} = new();
    }

    public class InnerErrorModel
    {
        public string Description{get;set;} = string.Empty;
    }
}