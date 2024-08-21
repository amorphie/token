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
    public class LocalTokenInfo
    {
        public bool active { get; set; }
    }

    public static class ResolveToken
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        public static async Task<IResult> resolveToken(
        [FromBody] dynamic body,
        [FromServices] IConfiguration configuration,
        [FromServices] DatabaseContext databaseContext
        )
        {
            dynamic variables = new ExpandoObject();
            variables.TokenInfo = new ExpandoObject();
            variables.TokenInfo.isExist = false;

            var jwt = body.GetProperty("accessToken").ToString();
            
            using var httpClient = new HttpClient();
            FormUrlEncodedContent content = new FormUrlEncodedContent([new KeyValuePair<string,string>("token",jwt)]);
            var httpResponse = await httpClient.PostAsync(configuration["Basepath"]+"/private/Introspect",content);
            if(httpResponse.IsSuccessStatusCode)
            {
                var response = await httpResponse.Content.ReadAsStringAsync();
                var tokenInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<LocalTokenInfo>(response);
                if(tokenInfo.active)
                {
                    string jti = JwtHelper.GetClaim(jwt,"jti");
                    var token = await databaseContext.Tokens.FirstOrDefaultAsync(t => t.Id.Equals(Guid.Parse(jti)) && t.TokenType == TokenType.AccessToken);
                    if(token is {} && token.IsActive)
                    {
                        variables.TokenInfo.data = token;
                        variables.TokenInfo.isExist = true;
                    }
                }
            }

            return Results.Ok(variables);
        }
    }
}