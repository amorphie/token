

namespace amorphie.token.Services.Client;

public class ClientServiceLocal : IClientService
{
    private readonly IHttpClientFactory _httpClientFactory;
    public ClientServiceLocal(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ServiceResponse<ClientResponse>> CheckClient(string clientId)
    {

        var httpClient = _httpClientFactory.CreateClient("Client");
        var httpResponseMessage = await httpClient.GetAsync(
            "client/" + clientId);
        if (httpResponseMessage.IsSuccessStatusCode)
        {
            var client = await httpResponseMessage.Content.ReadFromJsonAsync<ClientResponse>();
            if (client == null)
            {
                throw new ServiceException((int)Errors.InvalidClient, "Client not found with provided ClientId");
            }
            return new ServiceResponse<ClientResponse>()
            {
                StatusCode = 200,
                Response = client
            };
        }
        else
        {
            Console.WriteLine($"Client Status Code : {httpResponseMessage.StatusCode}");
            Console.WriteLine($"Client Status Code : {await httpResponseMessage.Content.ReadAsStringAsync()}");
            throw new ServiceException((int)Errors.InvalidClient, "Client Endpoint Did Not Response Successfully");
        }


    }

    public async Task<ServiceResponse<ClientResponse>> ValidateClient(string clientId, string clientSecret)
    {
        var httpClient = _httpClientFactory.CreateClient("Client");
        var httpResponseMessage = await httpClient.GetAsync(
            "client/" + clientId);
        if (httpResponseMessage.IsSuccessStatusCode)
        {
            var client = await httpResponseMessage.Content.ReadFromJsonAsync<ClientResponse>();
            if (client == null)
            {
                throw new ServiceException((int)Errors.InvalidClient, "Client not found with provided ClientId");
            }
            return new ServiceResponse<ClientResponse>()
            {
                StatusCode = 200,
                Response = client
            };
        }
        else
        {
            Console.WriteLine($"Client Status Code : {httpResponseMessage.StatusCode}");
            Console.WriteLine($"Client Status Code : {await httpResponseMessage.Content.ReadAsStringAsync()}");
            throw new ServiceException((int)Errors.InvalidClient, "Client Endpoint Did Not Response Successfully");
        }
    }
}
