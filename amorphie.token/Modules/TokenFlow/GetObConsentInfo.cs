using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using amorphie.token.data;
using amorphie.token.Services.Consent;
using Elastic.Apm.Api;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace amorphie.token.Modules.TokenFlow
{
    public static class GetObConsentInfo
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        public static async Task<IResult> getObConsentInfo(
        [FromBody] dynamic body,
        [FromServices] IConsentService consentService
        )
        {
            dynamic variables = new ExpandoObject();
            variables.ConsentInfo = new ExpandoObject();

            Guid consentId = body.GetProperty("id").ToString();
            var consentResponse = await consentService.GetConsent(consentId);

            if (consentResponse.StatusCode != 200)
            {
                variables.ConsentInfo.isExist = false;
                variables.ConsentInfo.errorCode = consentResponse.StatusCode;
                variables.ConsentInfo.errorMessage = consentResponse.Detail;
                return Results.Ok(variables);
            }
            var consent = consentResponse.Response;
            
            variables.ConsentInfo.isExist = true;
            variables.ConsentInfo.data = consent;

            return Results.Ok(variables);
        }
    }
}