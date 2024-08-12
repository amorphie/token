
using System.Dynamic;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace amorphie.token.Modules.OtpProcess
{
    public static class OtpFlow
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        public static async Task<IResult> otpFlow(
        [FromBody] dynamic body,
        IConfiguration configuration,
        DaprClient daprClient
        )
        {
            var transactionId = body.GetProperty("InstanceId").ToString();
            var phone = body.GetProperty("AmorphieOtpProcessRequest").GetProperty("phone");
            string message = body.GetProperty("AmorphieOtpProcessRequest").GetProperty("message").ToString();
            
            var rand = new Random();
            var code = String.Empty;

            for (int i = 0; i < 6; i++)
            {
                code += rand.Next(10);
            }

            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (env != null && !env.Equals("Prod"))
            code = "123456";

            await daprClient.SaveStateAsync(configuration["DAPR_STATE_STORE_NAME"], $"{transactionId}_ThirdFactor_Otp_Code", code, metadata: new Dictionary<string, string> { { "ttlInSeconds", "180" } });

            var otpRequest = new
            {
                Sender = "AutoDetect",
                SmsType = "Otp",
                Phone = new
                {
                    CountryCode = phone.GetProperty("countryCode").ToString(),
                    Prefix = phone.GetProperty("prefix").ToString(),
                    Number = phone.GetProperty("number").ToString()
                },
                Content = message.Replace("[[OtpValue]]",code),
                Process = new
                {
                    Name = "Third Factor Otp Flow",
                    Identity = "Amorphie-Otp-Process"
                }
            };

            StringContent request = new(JsonSerializer.Serialize(otpRequest), Encoding.UTF8, "application/json");

            using var httpClient = new HttpClient();
            var httpResponse = await httpClient.PostAsync(configuration["MessagingGatewayUri"], request);

            if (httpResponse.IsSuccessStatusCode)
            {
                return Results.Json(new{OtpProcessTryCount=0},new JsonSerializerOptions{PropertyNamingPolicy=null});
            }
            else
            {
                return Results.Ok();
            }
                
        }
    }
}