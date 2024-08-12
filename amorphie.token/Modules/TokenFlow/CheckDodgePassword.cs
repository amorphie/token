using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using amorphie.token.core.Models.InternetBanking;
using amorphie.token.data;
using amorphie.token.Services.Consent;
using amorphie.token.Services.InternetBanking;
using Elastic.Apm.Api;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace amorphie.token.Modules.TokenFlow
{
    public class CheckDodgePasswordRequest
    {
        public IBPassword Password{get;set;}
        public string ProvidedPassword{get;set;}
    }

    public static class CheckDodgePassword
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        public static async Task<IResult> checkDodgePassword(
        [FromBody] CheckDodgePasswordRequest checkDodgePasswordRequest,
        [FromServices] IInternetBankingUserService userService
        )
        {
            dynamic variables = new ExpandoObject();

            var checkPasswordResult = userService.VerifyPassword(checkDodgePasswordRequest.Password.HashedPassword!, checkDodgePasswordRequest.ProvidedPassword, checkDodgePasswordRequest.Password.Id.ToString());
            if(checkPasswordResult == PasswordVerificationResult.Failed)
            {
                variables.PasswordMatch = false;
            }
            variables.PasswordMatch = true;

            return Results.Ok(variables);
        }
    }
}