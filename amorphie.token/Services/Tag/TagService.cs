using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using amorphie.token.Services;
using Dapr.Client;

namespace amorphie.token.Services.Tag;

public class TagService : ServiceBase,ITagService
{
    private readonly DaprClient _daprClient;
    public TagService(DaprClient daprClient,ILogger<TagService> logger,IConfiguration configuration):base(logger,configuration)
    {
        _daprClient = daprClient;
    }
    public async Task<ServiceResponse<Dictionary<string,dynamic>>> GetTagInfo(string domain, string entity, string tagName, string queryString)
    {
        try
        {
            var tagData = await _daprClient.InvokeMethodAsync<Dictionary<string,dynamic>>(HttpMethod.Get,Configuration["TagExecutionServiceAppName"],$"tag/{domain}/{entity}/{tagName}/execute/{queryString}");

            return new ServiceResponse<Dictionary<string,dynamic>> { StatusCode = 200, Response = tagData};
        }
        catch(InvocationException ex)
        {
            return new ServiceResponse<Dictionary<string,dynamic>> { StatusCode = (int)ex.Response.StatusCode, Detail = ex.ToString()};
        }
        catch (System.Exception ex)
        {
            return new ServiceResponse<Dictionary<string,dynamic>> { StatusCode = 500, Detail = ex.ToString()};
        }
        return null;
    }
}
