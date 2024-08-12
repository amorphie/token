using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using amorphie.token.core.Models.Client;
using amorphie.token.core.Models.User;

namespace amorphie.token.core.Models.Authorization
{
    public class PreLoginInfo
    {
        public required LoginResponse User{get;set;}
        public required ClientResponse Client{get;set;}
    }
}