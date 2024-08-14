

namespace amorphie.token.Services.Tag;

public class TagServiceLocal : ITagService
{
    private readonly IHttpClientFactory _httpClientFactory;
    public TagServiceLocal(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ServiceResponse<Dictionary<string,dynamic>>> GetTagInfo(string domain, string entity, string tagName, string queryString)
    {
        var httpClient = _httpClientFactory.CreateClient("Tag");
        var httpResponseMessage = await httpClient.GetAsync(
            $"tag/{domain}/{entity}/{tagName}/execute{queryString}");

        if(httpResponseMessage.IsSuccessStatusCode)
        {
            var tag = await httpResponseMessage.Content.ReadFromJsonAsync<Dictionary<string,dynamic>>();
            if(tag == null)
            {
                return new ServiceResponse<Dictionary<string,dynamic>> { StatusCode = 404, Detail = "Tag Not Found"};
            }         
            return new ServiceResponse<Dictionary<string, dynamic>>{StatusCode = 200, Response = tag};   
        }
        else
        {
            return new ServiceResponse<Dictionary<string,dynamic>> { StatusCode = (int)httpResponseMessage.StatusCode, Detail = await httpResponseMessage.Content.ReadAsStringAsync()};
        }
    }
}
