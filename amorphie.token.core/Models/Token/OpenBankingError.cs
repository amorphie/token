using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace amorphie.token.core.Models.Token
{
    public class OpenBankingError
    {
        public required int HttpCode{get;set;}
        public required string HttpMessage{get;set;}
        public required string ErrorCode{get;set;}
        public required string MoreInformation{get;set;}
        public required string MoreInformationTr{get;set;}
    }
}