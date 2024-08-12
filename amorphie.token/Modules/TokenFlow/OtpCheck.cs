using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using amorphie.token.core.Models.InternetBanking;
using amorphie.token.core.Models.Profile;
using amorphie.token.data;
using amorphie.token.Services.Consent;
using amorphie.token.Services.InternetBanking;
using Elastic.Apm.Api;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace amorphie.token.Modules.TokenFlow
{
    
    public static class OtpCheck
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        public static async Task<IResult> otpCheck(
        [FromBody] dynamic body,
        [FromServices] DaprClient daprClient,
        [FromServices] IConfiguration configuration
        )
        {
            dynamic variables = new ExpandoObject();

            var id = body.GetProperty("id").ToString();
            var providedOtp = body.GetProperty("providedOtp").ToString();
            
            var sendedOtp = await daprClient.GetStateAsync<string>(configuration["DAPR_STATE_STORE_NAME"], $"{id}_Flow_Otp_Code");

            if (sendedOtp.Equals(providedOtp))
            {
                variables.IsOtpMatch = true;
            }
            else
            {
                variables.IsOtpMatch = false;
            }
 

            return Results.Ok(variables);
        }
    }
}