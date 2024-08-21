
using System.Dynamic;
using System.Text.Json;
using amorphie.token.Services.FlowHandler;

namespace amorphie.token.Filters
{
    public class FlowProcessFilter : IEndpointFilter
    {        
        public async Task<string> GetFlowInstanceId(HttpContext context)
        {
            var body = context.Request.Body;
            body.Seek(0, SeekOrigin.Begin);

            string rawBody = "";
            using (var bodyReader = new StreamReader(body))
            {
                rawBody = await bodyReader.ReadToEndAsync();
            }

            var dynamicBody = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(rawBody);
            return dynamicBody.FlowInstanceId;
        }
        
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            
            var flowInstanceId = await GetFlowInstanceId(context.HttpContext);
            var daprClient = context.HttpContext.RequestServices.GetService<DaprClient>();
            var configuration = context.HttpContext.RequestServices.GetService<IConfiguration>();
            var flowHandler = context.HttpContext.RequestServices.GetService<IFlowHandler>();
            var instanceKey = Convert.ToInt64(context.HttpContext.Request.Headers.FirstOrDefault(h => h.Key.Equals("X-Zeebe-Process-Instance-Key")).Value);
            var jobKey = Convert.ToInt64(context.HttpContext.Request.Headers.FirstOrDefault(h => h.Key.Equals("X-Zeebe-Job-Key")).Value);

            await flowHandler!.Init(flowInstanceId);
        
            try
            {
                var result = await next(context);
                return result;
            }
            catch (Exception ex)
            {
                dynamic variables = new ExpandoObject();
                variables.jobKey = jobKey;
                variables.errorCode = "amorphie-refresh-token-flow-error";
                variables.errorMessage = ex.ToString();
                await daprClient!.InvokeBindingAsync(configuration!["ZeebeCommand"], "throw-error", variables);
            }

            return null;
        }
    }

}