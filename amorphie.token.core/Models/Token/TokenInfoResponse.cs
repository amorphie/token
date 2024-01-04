using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace amorphie.token.core.Models.Token;

public class TokenInfoResponse
{
    public bool Active { get; set; }
    public string Scope { get; set; }
    public string ClientId { get; set; }
    public string Reference { get; set; }
    public DateTime ExpiredAt { get; set; }
}
