using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace amorphie.token.core.Models.Account;

public class User
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string Name { get; set; }
}
