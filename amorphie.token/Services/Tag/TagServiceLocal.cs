

namespace amorphie.token.Services.Tag;

public class TagServiceLocal : ITagService
{
    private readonly IHttpClientFactory _httpClientFactory;
    public TagServiceLocal(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<Dictionary<string, dynamic>> GetTagInfo(string domain, string entity, string tagName, string queryString)
    {
        var httpClient = _httpClientFactory.CreateClient("Tag");
        var httpResponseMessage = await httpClient.GetAsync(
            $"tag/{domain}/{entity}/{tagName}{queryString}");

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            var user = await httpResponseMessage.Content.ReadFromJsonAsync<dynamic>();
            if (user == null)
            {
                throw new ServiceException((int)Errors.InvalidUser, "Tag not found with provided info");
            }
            return user;
        }
        else
        {
            throw new ServiceException((int)Errors.InvalidUser, "Tag Endpoint Did Not Response Successfully");
        }
    }
}
