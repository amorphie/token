using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using amorphie.token.core.Enums;
using Microsoft.EntityFrameworkCore;

namespace amorphie.token.core.Models.Token
{
    [Index(nameof(Reference))]
    public class FailedLogon
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? Reference { get; set; }
        public string ClientId { get; set; }
        public Guid LogonId { get; set; }
        public Logon Logon { get; set; }
    }
}