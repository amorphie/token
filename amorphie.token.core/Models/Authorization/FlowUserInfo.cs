using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace amorphie.token.core.Models.Authorization
{
    public class FlowUserInfo
    {
        public required string deviceId { get; set; }
        public required string tokenId { get; set; }
    }
}