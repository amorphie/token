
using System.Dynamic;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace amorphie.token.Modules;

public static class CheckPush
{
    public static void MapCheckPushControlEndpoints(this WebApplication app)
    {
        app.MapPost("/amorphie-token-check-push-login-flow", checkPush)
        .Produces(StatusCodes.Status200OK);

        static async Task<IResult> checkPush(
        [FromBody] dynamic body,
        [FromServices] IAuthorizationService authorizationService,
        IConfiguration configuration,
        DaprClient daprClient
        )
        {
            Console.WriteLine("CheckPush Called");
            var transactionId = body.GetProperty("InstanceId").ToString();
            var entityData = body.GetProperty("TRX-send-push-login-flow").GetProperty("Data").GetProperty("entityData").ToString();

            var entityObj = JsonSerializer.Deserialize<Dictionary<string, object>>(entityData);
            var providedCode = entityObj["otpValue"].ToString();

            var generatedCode = await daprClient.GetStateAsync<string>(configuration["DAPR_STATE_STORE_NAME"], $"{transactionId}_Login_Otp_Code");

            if (providedCode == generatedCode)
            {
                dynamic variables = new ExpandoObject();
                variables.status = true;
                Console.WriteLine("CheckPush Success");
                return Results.Ok(variables);
            }
            else
            {
                dynamic variables = new ExpandoObject();
                variables.status = false;
                variables.message = "Push Check Failed";
                variables.LastTransition = "token-error";
                Console.WriteLine("CheckPush Error "+JsonSerializer.Serialize(variables));
                return Results.Ok(variables);
            }
        }

    }
}