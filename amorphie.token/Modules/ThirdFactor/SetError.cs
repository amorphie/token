
using System.Dynamic;
using System.Text;
using System.Text.Json;
using amorphie.token.core.Constants;
using Microsoft.AspNetCore.Mvc;

namespace amorphie.token.Modules.ThirdFactor;

public static class SetError
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public static async Task<IResult> setError(
    [FromBody] dynamic body,
    IConfiguration configuration
    )
    {
        dynamic variables = new ExpandoObject();
        variables.AmorphieThirdFactorResponse = new ExpandoObject();
        variables.AmorphieThirdFactorResponse.status = "Error";
        variables.AmorphieThirdFactorResponse.errorCode = body.GetProperty("errorCode").ToString();
        variables.AmorphieThirdFactorResponse.errorMessage = body.GetProperty("errorMessage").ToString();
        return Results.Ok(variables);
    }


}
