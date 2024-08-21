using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using amorphie.token.core.Models.Consent;

namespace amorphie.token.Services.TransactionHandler
{
    public interface ITransactionService
    {
        public string IpAddress { get; set; }
        public Logon Logon { get; set; }
        public int RoleKey{get;set;}
        public ClientResponse? Client{ get; set; }
        public Task InitLogon(long instanceKey, long jobKey);
        public Task SaveLogon();
    }
}