using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using amorphie.token.data;
using amorphie.token.Services.Consent;
using amorphie.token.Services.InternetBanking;
using Elastic.Apm.Api;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace amorphie.token.Modules.TokenFlow
{
    public static class GetDodgeUserInfo
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        public static async Task<IResult> getDodgeUserInfo(
        [FromBody] dynamic body,
        [FromServices] IInternetBankingUserService userService
        )
        {
            dynamic variables = new ExpandoObject();
            variables.DodgeUserInfo = new ExpandoObject();

            string reference = body.GetProperty("reference").ToString();
            var dodgeUserResponse = await userService.GetUser(reference);

            if (dodgeUserResponse.StatusCode != 200)
            {
                variables.DodgeUserInfo.isExist = false;
                variables.DodgeUserInfo.errorCode = dodgeUserResponse.StatusCode;
                variables.DodgeUserInfo.errorMessage = dodgeUserResponse.Detail;
                return Results.Ok(variables);
            }
            var user = dodgeUserResponse.Response;
            
            variables.DodgeUserInfo.isExist = true;
            variables.DodgeUserInfo.data = user;

            return Results.Ok(variables);
        }
    }
}