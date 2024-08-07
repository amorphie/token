using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using amorphie.token.core.Models.InternetBanking;

namespace amorphie.token.Services.InternetBanking
{
    public interface IInternetBankingUserService
    {
        public PasswordVerificationResult VerifyPassword(string username, string password, string salt);
        public Task<ServiceResponse<IBUser>> GetUser(string username);
        public Task<ServiceResponse<IBUser>> MordorGetUser(string username);
        public Task<ServiceResponse<IBPassword>> GetPassword(Guid Id);
        public Task<ServiceResponse<UserInfo>> GetAmorphieUserFromDodge(string username);
    }
}