using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using amorphie.token.core.Enums;
using Microsoft.EntityFrameworkCore;

namespace amorphie.token.core.Models.Token
{
    [Index(nameof(Code))]
    [Index(nameof(User))]
    public class AuthCode
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Code{get;set;}
        public DateTime CreatedAt{get;set;} = DateTime.UtcNow;
        public DateTime ExpiredAt{get;set;}
        public string Client{get;set;}
        [MaxLength(12)]
        public string? User{get;set;}

    }
}