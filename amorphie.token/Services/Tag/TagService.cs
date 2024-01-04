using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using amorphie.token.Services;
using Dapr.Client;

namespace amorphie.token.Services.Tag;

public class TagService : ServiceBase, ITagService
{
    private readonly DaprClient _daprClient;
    public TagService(DaprClient daprClient, ILogger<TagService> logger, IConfiguration configuration) : base(logger, configuration)
    {
        _daprClient = daprClient;
    }
    public async Task<Dictionary<string, dynamic>> GetTagInfo(string domain, string entity, string tagName, string queryString)
    {
        try
        {
            var tagData = await _daprClient.InvokeMethodAsync<Dictionary<string, dynamic>>(HttpMethod.Get, Configuration["TagExecutionServiceAppName"], $"tag/{domain}/{entity}/{tagName}/execute/{queryString}");
            Logger.LogInformation("Tag Data Path :" + $"tag/{domain}/{entity}/{tagName}/execute/{queryString}");
            Logger.LogInformation("Tag Data Serialized :" + JsonSerializer.Serialize(tagData));
            return tagData;
        }
        catch (InvocationException ex)
        {
            Logger.LogError("Dapr Service Invocation Failed | Detail:" + ex.Message);
        }
        catch (System.Exception ex)
        {
            Logger.LogError("An Error Occured When Getting Tag Data | Detail:" + ex.Message);
        }
        return null;
    }
}
