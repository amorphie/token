
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using amorphie.token.core.Models.RememberPassword;
using Microsoft.Extensions.Options;
using Refit;

namespace amorphie.token;

public class PasswordRememberService : ServiceBase, IPasswordRememberService
{
    // private readonly CardValidationOptions options;
    private readonly IPasswordRememberCard _passwordRememberCard;
    private readonly DaprClient _daprClient;
    public PasswordRememberService(
        ILogger<PasswordRememberService> logger,
        IConfiguration configuration,
        DaprClient daprClient,
        IPasswordRememberCard passwordRememberCard) : base(logger, configuration)
    {

        _passwordRememberCard = passwordRememberCard;
        _daprClient = daprClient;
    }

    public async Task<ServiceResponse<bool>> HasCardAsync(string citizenshipNo)
    {

        using var httpClient = new HttpClient();
        StringContent request = new(JsonSerializer.Serialize(new CardValidationOptions
        {
            ClientId = Configuration["CardValidationClientId"],
            ClientSecret = Configuration["CardValidationClientSecret"],
            GrantType = "client_credentials",
            Scopes = new List<string>() { "retail-customer" }
        }), Encoding.UTF8, "application/json");
        var response = new ServiceResponse<bool>();
        var httpResponse = await httpClient.PostAsync(Configuration["CardValidationTokenBaseAddress"], request);
        if (!httpResponse.IsSuccessStatusCode)
        {
            response.StatusCode = 500;
            response.Detail = "Couldn't Get Token For Using Card Service";
            response.Response = false;
            return response;
        }
        var resp = await httpResponse.Content.ReadFromJsonAsync<TokenResponse>();
        try
        {
            var cardResponse = await _passwordRememberCard.GetCards(citizenshipNo, resp.TokenType + " " + resp.AccessToken);
            if (cardResponse is not null && cardResponse.Count > 0)
            {
                response.StatusCode = 200;
                response.Detail = "Customer has cards.";
                response.Response = true;
                return response;
            }
            else
            {
                response.StatusCode = 200;
                response.Detail = "Customer has not cards";
                response.Response = false;
                return response;
            }
        }
        catch (ApiException apiException)
        {
            response.StatusCode = (int)apiException.StatusCode;
            response.Detail = apiException.ToString();
            response.Response = false;
            return response;
        }
        catch (Exception ex)
        {
            response.StatusCode = 500;
            response.Detail = ex.ToString();
            response.Response = false;
            return response;
        }

    }

    public async Task<ServiceResponse<GetEkycSettings>> VideoCallAvailableAsync()
    {

        var token = await GetVideoCallTokenAsync();
        bool isVideoCallAvailable = false;
        bool isActive = false;

        if (token is not null)
        {
            using var httpClient = new HttpClient();
            var header = new AuthenticationHeaderValue("Bearer", token);

            httpClient.DefaultRequestHeaders.Authorization = header;

            var resp = await httpClient.GetAsync(Configuration["VideoCallAvailable"]);

            if (resp.IsSuccessStatusCode)
            {
                var response = await resp.Content.ReadFromJsonAsync<VideoCallAvailableResponse>();
                TimeSpan activeStartHour = TimeSpan.Parse(response.ActiveStartHour);
                TimeSpan activeDueHour = TimeSpan.Parse(response.ActiveDueHour);
                if (activeStartHour < DateTime.Now.TimeOfDay && DateTime.Now.TimeOfDay < activeDueHour)
                {
                    isVideoCallAvailable = true;
                    isActive = response.IsActive;
                }

            }


        }

        return new ServiceResponse<GetEkycSettings> { Response = new GetEkycSettings { VideoCallAvailable = isVideoCallAvailable, IsActive = isActive } };
    }


    private async Task<string> GetVideoCallTokenAsync()
    {

        var token = await _daprClient.GetStateAsync<string>(Configuration["DAPR_STATE_STORE_NAME"], "amorphie-videoCallToken");

        if (token is not null)
        {
            return token;
        }


        using var httpClient = new HttpClient();

        StringContent request = new(JsonSerializer.Serialize(new CardValidationOptions
        {
            ClientId = Configuration["VideoCallTokenClientId"],
            ClientSecret = Configuration["VideoCallTokenClientSecret"],
            GrantType = "client_credentials",
            Scopes = new List<string>() { "openId" }
        }), Encoding.UTF8, "application/json");


        var httpResponse = await httpClient.PostAsync(Configuration["VideoCallAvailableTokenUrl"], request);
        if (httpResponse.IsSuccessStatusCode)
        {
            var resp = await httpResponse.Content.ReadFromJsonAsync<TokenResponse>();

            if (resp is not null)
            {

                var metadata = new Dictionary<string, string>
            {
                { "ttlInSeconds", resp.ExpiresIn.ToString() }
            };

                // Save Redis 

                token = resp!.AccessToken;

                await _daprClient.SaveStateAsync<string>(Configuration["DAPR_STATE_STORE_NAME"], "amorphie-videoCallToken", resp!.AccessToken, metadata: metadata);
            }


        }



        return token;
    }
}
