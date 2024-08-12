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
    public class OtpFlowRequest
    {
        public string Id{get;set;}
        public string Message{get;set;}
        public SimpleProfilePhone Phone{get;set;}
    }

    public static class OtpFlow
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        public static async Task<IResult> otpFlow(
        [FromBody] OtpFlowRequest otpFlowRequest,
        [FromServices] DaprClient daprClient,
        [FromServices] IConfiguration configuration
        )
        {
            dynamic variables = new ExpandoObject();

            var rand = new Random();
            var code = String.Empty;

            for (int i = 0; i < 6; i++)
            {
                code += rand.Next(10);
            }

            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (env != null && !env.Equals("Prod"))
            code = "123456";
            
            await daprClient.SaveStateAsync(configuration["DAPR_STATE_STORE_NAME"], $"{otpFlowRequest.Id}_Flow_Otp_Code", code, metadata: new Dictionary<string, string> { { "ttlInSeconds", "180" } });

            var otpRequest = new
            {
                Sender = "AutoDetect",
                SmsType = "Otp",
                Phone = new
                {
                    CountryCode = otpFlowRequest.Phone.countryCode!,
                    Prefix = otpFlowRequest.Phone.prefix,
                    Number = otpFlowRequest.Phone.number
                },
                Content = otpFlowRequest.Message.Replace("[[OtpValue]]",code),
                Process = new
                {
                    Name = "Amorphie Otp Flow",
                    Identity = "Amorphie-Otp-Flow-"+otpFlowRequest.Id
                }
            };

            StringContent request = new(JsonSerializer.Serialize(otpRequest), Encoding.UTF8, "application/json");

            using var httpClient = new HttpClient();
            var httpResponse = await httpClient.PostAsync(configuration["MessagingGatewayUri"], request);

            if (httpResponse.IsSuccessStatusCode)
            {
                variables.IsOtpFlowSuccess = true;
            }
            else
            {
                variables.IsOtpFlowSuccess = false;
            }
 

            return Results.Ok(variables);
        }
    }
}