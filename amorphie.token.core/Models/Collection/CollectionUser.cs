using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using amorphie.token.core.Models.MessagingGateway;
using amorphie.token.core.Models.User;
using Refit;

namespace amorphie.token.core.Models.Collection
{
    public class User
    {
        public string Name{get;set;}
        public string Surname{get;set;}
        public string CitizenshipNo{get;set;}
        public string LoginUser{get;set;}
        public Role Role{get;set;}
        public string DepartmentCode{get;set;}
        public PhoneString Phone{get;set;}
        public string Mail{get;set;}
    }
}