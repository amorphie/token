using System.Text;
using System.Text.Json;
using amorphie.token.core.Dtos;
using MongoDB.Bson;

namespace amorphie.token.Services.User;

public class UserServiceLocal : IUserService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public UserServiceLocal(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ServiceResponse<object>> CheckDevice(Guid userId, string clientId, string deviceId,
        Guid installationId)
    {
        var httpClient = _httpClientFactory.CreateClient("User");
        var httpResponseMessage =
            await httpClient.GetAsync($"userDevice/check-device/{clientId}/{userId}/{deviceId}/{installationId}");

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            return new ServiceResponse<object>() { StatusCode = 200, Response = null };
        }
        else
        {
            return new ServiceResponse<object>() { StatusCode = 404, Response = "Device Not Found" };
        }
    }

    public async Task<ServiceResponse<CheckDeviceWithoutUserResponseDto>> CheckDeviceWithoutUser(string clientId,
        string deviceId, Guid installationId)
    {
        var httpClient = _httpClientFactory.CreateClient("User");
        var httpResponseMessage =
            await httpClient.GetAsync($"userDevice/check-device-without-user/{clientId}/{deviceId}/{installationId}");

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            var response = await httpResponseMessage.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<CheckDeviceWithoutUserResponseDto>(response);
            return new ServiceResponse<CheckDeviceWithoutUserResponseDto>()
                { StatusCode = 200, Response = responseObject };
        }
        else
        {
            return new ServiceResponse<CheckDeviceWithoutUserResponseDto>() { StatusCode = 404, Response = null };
        }
    }

    public async Task<ServiceResponse<LoginResponse>> Login(LoginRequest loginRequest)
    {
        var httpClient = _httpClientFactory.CreateClient("User");
        var httpResponseMessage = await httpClient.PostAsJsonAsync<LoginRequest>(
            "user/login", loginRequest);

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            var user = await httpResponseMessage.Content.ReadFromJsonAsync<LoginResponse>();
            if (user == null)
            {
                return new ServiceResponse<LoginResponse>() { StatusCode = 404  };
            }

            return new ServiceResponse<LoginResponse>() { StatusCode = 200, Response = user };
        }
        else
        {
            return new ServiceResponse<LoginResponse>() { StatusCode = (int)httpResponseMessage.StatusCode  };
        }
    }

    public async Task<ServiceResponse<LoginResponse>> GetUserById(Guid userId)
    {
        var httpClient = _httpClientFactory.CreateClient("User");
        var httpResponseMessage = await httpClient.GetAsync(
            "user/" + userId.ToString());

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            var user = await httpResponseMessage.Content.ReadFromJsonAsync<LoginResponse>();
            if (user == null)
            {
                throw new ServiceException((int)Errors.InvalidUser, "User not found with provided info");
            }

            return new ServiceResponse<LoginResponse>() { StatusCode = 200, Response = user };
        }
        else
        {
            throw new ServiceException((int)Errors.InvalidUser, "User Endpoint Did Not Response Successfully");
        }
    }

    public async Task<ServiceResponse> SaveUser(UserInfo userInfo)
    {
        var httpClient = _httpClientFactory.CreateClient("User");
        var request = new StringContent(JsonSerializer.Serialize(userInfo), Encoding.UTF8, "application/json");
        var httpResponseMessage = await httpClient.PostAsync(
            "user", request);

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            return new ServiceResponse() { StatusCode = 200 };
        }
        else
        {
            return new ServiceResponse() { StatusCode = (int)httpResponseMessage.StatusCode, Detail = "User Endpoint Did Not Response Successfully | " +await httpResponseMessage.Content.ReadAsStringAsync()};
        }
    }

    public async Task<ServiceResponse> SaveDevice(UserSaveMobileDeviceDto userSaveMobileDeviceDto)
    {
        var httpClient = _httpClientFactory.CreateClient("User");
        var request = new StringContent(JsonSerializer.Serialize(userSaveMobileDeviceDto), Encoding.UTF8,
            "application/json");
        var httpResponseMessage = await httpClient.PostAsync(
            "userDevice/save-mobile-device-client", request);

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            return new ServiceResponse() { StatusCode = 200 };
        }
        else
        {
            throw new ServiceException((int)Errors.InvalidUser, "User Endpoint Did Not Response Successfully");
        }
    }

    public async Task<ServiceResponse> RemoveDevice(string reference, string clientId)
    {
        var httpClient = _httpClientFactory.CreateClient("User");
        var httpResponseMessage = await httpClient.PutAsync(
            "public/device/remove/" + clientId + "/" + reference, null);

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            return new ServiceResponse() { StatusCode = 200 };
        }
        else
        {
            throw new ServiceException((int)Errors.InvalidUser, "User Endpoint Did Not Response Successfully");
        }
    }

    public async Task<ServiceResponse<LoginResponse>> GetUserByReference(string reference)
    {
        var httpClient = _httpClientFactory.CreateClient("User");
        var httpResponseMessage = await httpClient.GetAsync(
            "user/reference/" + reference.ToString());

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            var user = await httpResponseMessage.Content.ReadFromJsonAsync<LoginResponse>();
            if (user == null)
            {
                throw new ServiceException((int)Errors.InvalidUser, "User not found with provided info");
            }

            return new ServiceResponse<LoginResponse>() { StatusCode = 200, Response = user };
        }
        else
        {
            throw new ServiceException((int)Errors.InvalidUser, "User Endpoint Did Not Response Successfully");
        }
    }

    public async Task<ServiceResponse> MigrateSecurityQuestion(
        MigrateSecurityQuestionRequest migrateSecurityQuestionRequest)
    {
        var httpClient = _httpClientFactory.CreateClient("User");
        var request = new StringContent(JsonSerializer.Serialize(migrateSecurityQuestionRequest), Encoding.UTF8,
            "application/json");
        var httpResponseMessage = await httpClient.PostAsync(
            "userSecurityQuestion/migrate", request);

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            return new ServiceResponse() { StatusCode = 200 };
        }
        else
        {
            return new ServiceResponse()
                { StatusCode = (int)httpResponseMessage.StatusCode, Detail = "Migrate Security Question Error" };
        }
    }

    public async Task<ServiceResponse> MigrateSecurityImage(MigrateSecurityImageRequest migrateSecurityImageRequest)
    {
        var httpClient = _httpClientFactory.CreateClient("User");
        var request = new StringContent(JsonSerializer.Serialize(migrateSecurityImageRequest), Encoding.UTF8,
            "application/json");
        var httpResponseMessage = await httpClient.PostAsync(
            "userSecurityImage/migrate", request);

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            return new ServiceResponse() { StatusCode = 200 };
        }
        else
        {
            return new ServiceResponse()
                { StatusCode = (int)httpResponseMessage.StatusCode, Detail = "Migrate Security Image Error" };
        }
    }

    public async Task<ServiceResponse> MigrateSecurityQuestions(
        List<SecurityQuestionRequestDto> securityQuestionRequestDtos)
    {
        var httpClient = _httpClientFactory.CreateClient("User");
        var request = new StringContent(JsonSerializer.Serialize(securityQuestionRequestDtos), Encoding.UTF8,
            "application/json");
        var httpResponseMessage = await httpClient.PostAsync(
            "userSecurityQuestion/migrateQuestions", request);

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            return new ServiceResponse() { StatusCode = 200 };
        }
        else
        {
            return new ServiceResponse()
                { StatusCode = (int)httpResponseMessage.StatusCode, Detail = "Migrate Security Questions Error" };
        }
    }

    public async Task<ServiceResponse<GetPublicDeviceDto>> GetPublicDevice(string clientCode, string reference)
    {
        var httpClient = _httpClientFactory.CreateClient("User");
        var httpResponseMessage = await httpClient.GetAsync($"public/device/{clientCode}/{reference}");

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            var device = await httpResponseMessage.Content.ReadFromJsonAsync<GetPublicDeviceDto>();
            return new ServiceResponse<GetPublicDeviceDto>() { StatusCode = 200, Detail = "Success", Response = device };
        }

        return new ServiceResponse<GetPublicDeviceDto>() { StatusCode = 404, Detail = "Device Not Found"};
    }

    public async Task<ServiceResponse> MigrateSecurityImages(List<SecurityImageRequestDto> securityImageRequestDtos)
    {
        var httpClient = _httpClientFactory.CreateClient("User");
        var request = new StringContent(JsonSerializer.Serialize(securityImageRequestDtos), Encoding.UTF8,
            "application/json");
        var httpResponseMessage = await httpClient.PostAsync(
            "userSecurityImage/migrateImages", request);

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            return new ServiceResponse() { StatusCode = 200 };
        }
        else
        {
            return new ServiceResponse()
                { StatusCode = (int)httpResponseMessage.StatusCode, Detail = "Migrate Security Images Error" };
        }
    }

    public async Task<ServiceResponse<IEnumerable<SecurityQuestionDto>>> GetSecurityQuestions()
    {
        var httpClient = _httpClientFactory.CreateClient("User");
        var httpResponseMessage = await httpClient.GetAsync(
            "securityQuestion/getAll");

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            var questions = await httpResponseMessage.Content.ReadFromJsonAsync<IEnumerable<SecurityQuestionDto>>();
            if (questions == null)
            {
                return new ServiceResponse<IEnumerable<SecurityQuestionDto>>()
                    { StatusCode = 200, Response = questions };
            }

            return new ServiceResponse<IEnumerable<SecurityQuestionDto>>() { StatusCode = 200, Response = questions };
        }
        else
        {
            throw new ServiceException(500, "User Endpoint Did Not Response Successfully");
        }
    }

    public async Task<ServiceResponse<IEnumerable<SecurityImageDto>>> GetSecurityImages()
    {
        var httpClient = _httpClientFactory.CreateClient("User");
        var httpResponseMessage = await httpClient.GetAsync(
            "securityImage/getAll");

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            var images = await httpResponseMessage.Content.ReadFromJsonAsync<IEnumerable<SecurityImageDto>>();
            if (images == null)
            {
                return new ServiceResponse<IEnumerable<SecurityImageDto>>() { StatusCode = 404, Response = images };
            }

            return new ServiceResponse<IEnumerable<SecurityImageDto>>() { StatusCode = 200, Response = images };
        }
        else
        {
            throw new ServiceException(500, "User Endpoint Did Not Response Successfully");
        }
    }

    public async Task<ServiceResponse<UserSecurityQuestionDto>> GetLastSecurityQuestion(Guid id)
    {
        var httpClient = _httpClientFactory.CreateClient("User");
        var httpResponseMessage = await httpClient.GetAsync(
            "userSecurityQuestion/getLastSecurityQuestion/" + id);

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            var lastQuestion = await httpResponseMessage.Content.ReadFromJsonAsync<UserSecurityQuestionDto>();
            if (lastQuestion == null)
            {
                return new ServiceResponse<UserSecurityQuestionDto>() { StatusCode = 404, Response = lastQuestion };
            }

            return new ServiceResponse<UserSecurityQuestionDto>() { StatusCode = 200, Response = lastQuestion };
        }
        else
        {
            throw new ServiceException(500, "User Endpoint Did Not Response Successfully");
        }
    }

    public async Task<ServiceResponse<UserSecurityImageDto>> GetLastSecurityImage(Guid id)
    {
        var httpClient = _httpClientFactory.CreateClient("User");
        var httpResponseMessage = await httpClient.GetAsync(
            "userSecurityImage/getLastSecurityImage/" + id);

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            var lastImage = await httpResponseMessage.Content.ReadFromJsonAsync<UserSecurityImageDto>();
            if (lastImage == null)
            {
                return new ServiceResponse<UserSecurityImageDto>() { StatusCode = 404, Response = lastImage };
            }

            return new ServiceResponse<UserSecurityImageDto>() { StatusCode = 200, Response = lastImage };
        }
        else
        {
            throw new ServiceException(500, "User Endpoint Did Not Response Successfully");
        }
    }

    public async Task<ServiceResponse<IEnumerable<UserClaimDto>>> GetUserClaims(Guid userId)
    {
        var httpClient = _httpClientFactory.CreateClient("User");
        var httpResponseMessage = await httpClient.GetAsync(
            "userClaim/getByUserId/" + userId);

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            var userClaims = await httpResponseMessage.Content.ReadFromJsonAsync<IEnumerable<UserClaimDto>>();
            if (userClaims == null)
            {
                return new ServiceResponse<IEnumerable<UserClaimDto>>() { StatusCode = 404, Response = null };
            }

            return new ServiceResponse<IEnumerable<UserClaimDto>>() { StatusCode = 200, Response = userClaims };
        }
        else
        {
            throw new ServiceException(500, "User Endpoint Did Not Response Successfully");
        }
    }

    public async Task<ServiceResponse<ActiveDeviceDto>> CheckDevice(string reference, string clientId)
    {
        var httpClient = _httpClientFactory.CreateClient("User");
        var httpResponseMessage = await httpClient.GetAsync($"public/device/{clientId}/{reference}");

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            var raw = await httpResponseMessage.Content.ReadAsStringAsync();
            var device = JsonSerializer.Deserialize<ActiveDeviceDto>(raw,new JsonSerializerOptions{PropertyNameCaseInsensitive = true});
            return new ServiceResponse<ActiveDeviceDto>() { StatusCode = 200, Response = device };
        }
        else
        {
            return new ServiceResponse<ActiveDeviceDto>() { StatusCode = 404, Detail = "Device Not Found" };
        }
    }
}
