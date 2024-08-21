
using System.Dynamic;
using System.Text;
using System.Text.Json;
using amorphie.token.core.Constants;
using Microsoft.AspNetCore.Mvc;

namespace amorphie.token.Modules;

public static class CheckOtpFlow
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public static async Task<IResult> checkOtpFlow(
    [FromBody] dynamic body,
    IConfiguration configuration,
    DaprClient daprClient
    )
    {
        var langCode = ErrorHelper.GetLangCode(body);
        var transactionId = body.GetProperty("InstanceId").ToString();
        var transitionName = body.GetProperty("LastTransition").ToString();
        var entityData = body.GetProperty("TRX-" + transitionName).GetProperty("Data").GetProperty(WorkflowConstants.ENTITY_DATA_FIELD).ToString();

        var entityObj = JsonSerializer.Deserialize<Dictionary<string, object>>(entityData);
        var providedCode = entityObj["otpValue"].ToString();
        var generatedCode = await daprClient.GetStateAsync<string?>(configuration["DAPR_STATE_STORE_NAME"], $"{transactionId}_Login_Otp_Code");

        dynamic variables = new ExpandoObject();
        if (generatedCode == null)
        {
            variables.otpTimeout = true;
            variables.message = ErrorHelper.GetErrorMessage(LoginErrors.OtpTimeout, langCode);
            return Results.Ok(variables);
        }
        variables.otpTimeout = false;

        if (generatedCode != null && providedCode == generatedCode)
        {
            variables.otpMatch = true;

            return Results.Ok(variables);
        }
        else
        {
            int otpTryCount = Convert.ToInt32(body.GetProperty("OtpTryCount").ToString());
            variables.otpMatch = false;
            variables.OtpTryCount = ++otpTryCount;
            variables.message = ErrorHelper.GetErrorMessage(LoginErrors.WrongOtp, langCode);
            return Results.Ok(variables);
        }
    }


}
