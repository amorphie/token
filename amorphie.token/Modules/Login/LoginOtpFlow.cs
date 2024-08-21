
using System.Dynamic;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace amorphie.token.Modules.Login;

public static class LoginOtpFlow
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public static async Task<IResult> loginOtpFlow(
    [FromBody] dynamic body,
    [FromServices] IAuthorizationService authorizationService,
    IConfiguration configuration,
    DaprClient daprClient
    )
    {
        var transactionId = body.GetProperty("InstanceId").ToString();

        var userInfoSerialized = body.GetProperty("userSerialized").ToString();

        LoginResponse userInfo = JsonSerializer.Deserialize<LoginResponse>(userInfoSerialized, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var rand = new Random();
        var code = String.Empty;

        for (int i = 0; i < 6; i++)
        {
            code += rand.Next(10);
        }

        var aks = Environment.GetEnvironmentVariable("AKS_ENV");
        if (aks != null && aks.Equals("E"))
            code = "123456";

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (env != null && !env.Equals("Prod"))
            code = "123456";
            
        //For Default User    
        if (userInfo.Reference == "99999999998")
            code = "963579";
        

        await daprClient.SaveStateAsync(configuration["DAPR_STATE_STORE_NAME"], $"{transactionId}_Login_Otp_Code", code, metadata: new Dictionary<string, string> { { "ttlInSeconds", "180" } });

        dynamic variables = new ExpandoObject();
        variables.otpTimeout = false;
        if (aks == null || aks.Equals("H"))
        {
            var otpRequest = new
            {
                Sender = "AutoDetect",
                SmsType = "Otp",
                Phone = new
                {
                    CountryCode = userInfo.MobilePhone!.CountryCode,
                    Prefix = userInfo.MobilePhone.Prefix,
                    Number = userInfo.MobilePhone.Number
                },
                Content = $"{code} şifresi ile giriş yapabilirsiniz",
                Process = new
                {
                    Name = "Token Login Flow",
                    Identity = "Otp Login"
                }
            };

            StringContent request = new(JsonSerializer.Serialize(otpRequest), Encoding.UTF8, "application/json");

            using var httpClient = new HttpClient();
            var httpResponse = await httpClient.PostAsync(configuration["MessagingGatewayUri"], request);

            if (httpResponse.IsSuccessStatusCode)
            {
                variables.status = true;
                variables.OtpTryCount = 0;
                return Results.Ok(variables);
            }
            else
            {
                variables.status = false;
                variables.message = "Otp Service Error";
                return Results.Ok(variables);
            }
        }
        else
        {
            variables.status = true;
            return Results.Ok(variables);
        }
    }


}
