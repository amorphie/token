using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace amorphie.token.core.Models.Account;

public class Login
{
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? RedirectUri { get; set; }
    public string? Code { get; set; }
    public IList<string>? RequestedScopes { get; set; }
    public string? OpenBanking { get; set; }
    public Guid TransactionId { get; set; }
}
