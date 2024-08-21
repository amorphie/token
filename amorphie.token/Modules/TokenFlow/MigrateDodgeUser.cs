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
using amorphie.token.Services.Login;
using Elastic.Apm.Api;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace amorphie.token.Modules.TokenFlow
{
   
    public static class MigrateDodgeUser
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        public static async Task<IResult> migrateDodgeUser(
        [FromBody] dynamic body,
        [FromServices] ILoginService loginService
        )
        {
            dynamic variables = new ExpandoObject();
            
            string reference = body.GetProperty("reference").ToString();
            await loginService.MigrateDodgeUserToAmorphie(reference);

            return Results.Ok(variables);
        }
    }
}