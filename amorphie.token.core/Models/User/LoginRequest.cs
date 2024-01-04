using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace amorphie.token.core.Models.User;

public class LoginRequest
{
    public string Reference { get; set; }
    public string Password { get; set; }
}
