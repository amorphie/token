
using System.Dynamic;
using System.Text;
using System.Text.Json;
using amorphie.token.core.Constants;
using Microsoft.AspNetCore.Mvc;

namespace amorphie.token.Modules.OtpProcess;

public static class SetError
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public static async Task<IResult> setError(
    [FromBody] dynamic body,
    IConfiguration configuration
    )
    {
        dynamic variables = new ExpandoObject();
        variables.AmorphieOtpProcessResponse = new ExpandoObject();
        variables.AmorphieOtpProcessResponse.status = "Error";
        variables.AmorphieOtpProcessResponse.errorCode = body.GetProperty("errorCode").ToString();
        variables.AmorphieOtpProcessResponse.errorMessage = body.GetProperty("errorMessage").ToString();
        return Results.Ok(variables);
    }


}
