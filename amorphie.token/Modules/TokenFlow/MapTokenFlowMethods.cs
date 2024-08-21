

using System.Dynamic;
using amorphie.token.Filters;
using Microsoft.AspNetCore.Mvc;

namespace amorphie.token.Modules.TokenFlow
{
    public static class MapTokenFlowMethods
    {
        public static void MapTokenFlowEndpoints(this WebApplication app)
        {
            var tokenFlowMethods = app.MapGroup(string.Empty).AddEndpointFilter<FlowProcessFilter>();

            app.MapGet("/filter-test", () => {
                Console.WriteLine("Method executed");
            })
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            tokenFlowMethods.MapPost("/amorphie-get-refresh-token-info", GetRefreshTokenInfo.getRefreshTokenInfo)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-get-ob-consent", GetObConsentInfo.getObConsentInfo)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            tokenFlowMethods.MapPost("/amorphie-get-client-info", GetClientInfo.getClientInfo)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-get-user-info", GetUserInfo.getUserInfo)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-get-profile-info", GetSimpleProfileInfo.getSimpleProfileInfo)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-get-dodge-user-info", GetDodgeUserInfo.getDodgeUserInfo)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-get-dodge-password-info", GetDodgeUserPassword.getDodgeUserPassword)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-check-dodge-password", CheckDodgePassword.checkDodgePassword)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-migrate-dodge-user", MigrateDodgeUser.migrateDodgeUser)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-otp-flow", OtpFlow.otpFlow)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-otp-check", OtpCheck.otpCheck)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-generate-auth-code", GenerateAuthCode.generateAuthCode)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-set-hub-data", SetHubData.setHubData)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-get-device-info", CheckDevice.checkDevice)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-get-token-info", ResolveToken.resolveToken)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-get-dodge-user-role-info", GetDodgeUserRoleInfo.getDodgeUserRoleInfo)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-generate-tokens-with-refresh-token", GenerateTokensWithRefreshToken.generateTokensWithRefreshToken)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            tokenFlowMethods.MapPost("/amorphie-validate-token", ValidateToken.validateToken)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            tokenFlowMethods.MapPost("/amorphie-flow-set-error", SetError.setError)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            tokenFlowMethods.MapPost("/amorphie-flow-set-success", SetSuccess.setSuccess)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            tokenFlowMethods.MapPost("/start-flow",async ([FromServices]DaprClient daprClient,[FromBody]dynamic body) => {
                dynamic data = new ExpandoObject();
                data.messageName = "amorphie-refresh-token-flow";
                data.variables = new ExpandoObject();
                data.variables = body;
                await daprClient.InvokeBindingAsync("zeebe-local","publish-message",data);
                return Results.Ok();
            });
            
            
        }



    }
}