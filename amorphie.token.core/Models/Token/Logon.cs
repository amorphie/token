using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using amorphie.token.core.Enums;
using Microsoft.EntityFrameworkCore;

namespace amorphie.token.core.Models.Token
{
    [Index(nameof(WorkflowInstanceId))]
    [Index(nameof(Reference))]
    public class Logon
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public LogonType LogonType { get; set; }
        public string? Reference { get; set; }
        public string ClientId { get; set; }
        public long WorkflowInstanceId { get; set; }
        public long LastJobKey { get; set; }
        public LogonStatus LogonStatus { get; set; }
        public string? Error { get; set; }
        public ICollection<FailedLogon>? FailedLogons { get; set; } = new List<FailedLogon>();
    }
}