
using System.Dynamic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using amorphie.token.data;
using amorphie.token.Services.FlowHandler;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace amorphie.token.Modules.TokenFlow
{
   
    public static class SetSuccess
    {
        public class SuccessModel
        {
            public dynamic result{get;set;}
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public static async Task<IResult> setSuccess(
        [FromBody]  SuccessModel successModel,
        [FromServices] IFlowHandler flowHandler
        )
        {
            var flowProcess = flowHandler.FlowProcess;
            flowProcess.StatusCode = 200;
            flowProcess.Result = successModel.result;
            flowProcess.FlowStatus = FlowStatus.Completed;
            await flowHandler.Save(flowProcess);

            dynamic variables = new ExpandoObject();
            variables.FlowHasError = true;
            return Results.Ok(variables);
        }
    }
}