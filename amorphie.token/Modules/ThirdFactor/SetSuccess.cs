
using System.Dynamic;
using System.Text;
using System.Text.Json;
using amorphie.token.core.Constants;
using Microsoft.AspNetCore.Mvc;

namespace amorphie.token.Modules.ThirdFactor;

public static class SetSuccess
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public static async Task<IResult> setSuccess(
    [FromBody] dynamic body,
    IConfiguration configuration
    )
    {
        dynamic variables = new ExpandoObject();
        variables.AmorphieThirdFactorResponse = new ExpandoObject();
        variables.AmorphieThirdFactorResponse.status = "Success";
        return Results.Ok(variables);
    }


}
