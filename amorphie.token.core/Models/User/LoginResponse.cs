using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using amorphie.token.core.Models.Profile;

namespace amorphie.token.core.Models.User;

public class LoginResponse
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    // public string Password { get; set; } = string.Empty;
    public string EMail { get; set; } = string.Empty;
    public Phone? MobilePhone { get; set; } = new Phone();

    public string Reference { get; set; } = string.Empty;
    public string State {get;set;}
    public Guid Id{get;set;}

    public ProfileResponse profile { get; set; }
   
}

public class Phone
{
    public int CountryCode{get;set;}
    public int Prefix{get;set;}
    public string Number{get;set;}

    public override string ToString()
    {
        return $"{CountryCode}{Prefix}{Number}";
    }
}
