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
    public static class GetDodgeUserPassword
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        public static async Task<IResult> getDodgeUserPassword(
        [FromBody] dynamic body,
        [FromServices] IInternetBankingUserService userService
        )
        {
            dynamic variables = new ExpandoObject();

            Guid userId = Guid.Parse(body.GetProperty("id").ToString());
            
            var passwordResponse = await userService.GetPassword(userId);
            if (passwordResponse.StatusCode != 200)
            {
                //TODO
                return Results.Ok(variables);
            }

            variables.PasswordInfo = passwordResponse.Response;

            return Results.Ok(variables);
        }
    }
}