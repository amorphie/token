using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using amorphie.core.Base;
using amorphie.token.core.Extensions;
using amorphie.token.core.Models.InternetBanking;
using amorphie.token.core.Models.Profile;
using amorphie.token.data;
using amorphie.token.Services.Consent;
using Microsoft.AspNetCore.Mvc;

namespace amorphie.token.Modules.Login
{
    public static class SaveConsent
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        public static async Task<IResult> saveConsent(
        [FromBody] dynamic body,
        [FromServices] IConsentService consentService,
        [FromServices] IConfiguration configuration
        )
        {
            var transitionName = body.GetProperty("LastTransition").ToString();

            var clientInfoSerialized = body.GetProperty("clientSerialized").ToString();

            ClientResponse clientInfo = JsonSerializer.Deserialize<ClientResponse>(clientInfoSerialized, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var userInfoSerialized = body.GetProperty("userSerialized").ToString();

            LoginResponse userInfo = JsonSerializer.Deserialize<LoginResponse>(userInfoSerialized, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            dynamic variables = new Dictionary<string, dynamic>();

            var saveConsent = await consentService.SaveConsent(clientInfo.id!, "7b19daa2-8793-45d2-9d96-aa7540c9d1ab", userInfo.Reference);
            if (saveConsent.StatusCode != 200)
            {
                //TODO
            }
            variables.Add("consentSaved", true);
            return Results.Ok(variables);
        }
    }
}