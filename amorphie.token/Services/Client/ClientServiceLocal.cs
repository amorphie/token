

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
                return new ServiceResponse<ClientResponse>(){ StatusCode = 404 , Detail = "Client not found with provided ClientId"};
            }
            return new ServiceResponse<ClientResponse>()
            {
                StatusCode = 200,
                Response = client
            };
        }
        else
        {
            return new ServiceResponse<ClientResponse>(){ StatusCode = (int)httpResponseMessage.StatusCode , Detail = "Client Endpoint Error"};
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
                return new ServiceResponse<ClientResponse>(){ StatusCode = 404 , Detail = "Client not found with provided ClientId"};
            }
            return new ServiceResponse<ClientResponse>()
            {
                StatusCode = 200,
                Response = client
            };
        }
        else
        {
            return new ServiceResponse<ClientResponse>(){ StatusCode = (int)httpResponseMessage.StatusCode , Detail = "Client Endpoint Error"};
        }
    }

    public async Task<ServiceResponse<ClientResponse>> CheckClientByCode(string clientNo)
    {

        var httpClient = _httpClientFactory.CreateClient("Client");
        var httpResponseMessage = await httpClient.GetAsync(
            "client/code/" + clientNo);
        if (httpResponseMessage.IsSuccessStatusCode)
        {
            var client = await httpResponseMessage.Content.ReadFromJsonAsync<ClientResponse>();
            if (client == null)
            {
                return new ServiceResponse<ClientResponse>(){ StatusCode = 404 , Detail = "Client not found with provided ClientId"};
            }
            return new ServiceResponse<ClientResponse>()
            {
                StatusCode = 200,
                Response = client
            };
        }
        else
        {
            return new ServiceResponse<ClientResponse>(){ StatusCode = (int)httpResponseMessage.StatusCode , Detail = "Client Endpoint Error"};
        }


    }

    public async Task<ServiceResponse<ClientResponse>> ValidateClientByCode(string clientNo, string clientSecret)
    {
        var httpClient = _httpClientFactory.CreateClient("Client");
        var httpResponseMessage = await httpClient.GetAsync(
            "client/code/" + clientNo);
        if (httpResponseMessage.IsSuccessStatusCode)
        {
            var client = await httpResponseMessage.Content.ReadFromJsonAsync<ClientResponse>();
            if (client == null)
            {
                return new ServiceResponse<ClientResponse>(){ StatusCode = 404 , Detail = "Client not found with provided ClientId"};
            }
            return new ServiceResponse<ClientResponse>()
            {
                StatusCode = 200,
                Response = client
            };
        }
        else
        {
            return new ServiceResponse<ClientResponse>(){ StatusCode = (int)httpResponseMessage.StatusCode , Detail = "Client Endpoint Error"};
        }
    }
}
