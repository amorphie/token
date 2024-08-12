
using System.Dynamic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using amorphie.token.data;
using amorphie.token.Services.FlowHandler;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace amorphie.token.Modules.TokenFlow
{
   
    public static class SetError
    {
        public class ErrorModel
        {
            public int StatusCode{get;set;}
            public string Message{get;set;}
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public static async Task<IResult> setError(
        [FromBody]  ErrorModel errorModel,
        [FromServices] IFlowHandler flowHandler
        )
        {
            var flowProcess = flowHandler.FlowProcess;
            flowProcess.StatusCode = errorModel.StatusCode;
            flowProcess.ErrorMessage = errorModel.Message;
            flowProcess.FlowStatus = FlowStatus.Error;
            await flowHandler.Save(flowProcess);

            dynamic variables = new ExpandoObject();
            variables.FlowHasError = true;
            return Results.Ok(variables);
        }
    }
}