using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace amorphie.token.core.Dtos
{
    public class UserClaimDto
    {
        public string ClaimName{get;set;}
        public string ClaimValue{get;set;}
    }

    public class SaveUserClaimDto
    {
        public Guid Id {get;set;} = Guid.NewGuid();
        public string ClaimName{get;set;}
        public string ClaimValue{get;set;}
        public string UserId{get;set;}
    }
}