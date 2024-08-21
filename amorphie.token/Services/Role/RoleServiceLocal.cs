using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using amorphie.token.core.Models.Role;

namespace amorphie.token.Services.Role
{
    public class RoleServiceLocal : ServiceBase, IRoleService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public RoleServiceLocal(ILogger<RoleServiceLocal> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory) : base(logger, configuration)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ServiceResponse<IEnumerable<ConsentDto>>> GetConsents(string clientCode, string reference)
        {
            var httpClient = _httpClientFactory.CreateClient("Consent");

            var httpResponseMessage = await httpClient.GetAsync(
                $"consent/GetUserConsents/clientCode={clientCode}&userTCKN={reference}");

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var consentResponse = await httpResponseMessage.Content.ReadFromJsonAsync<IEnumerable<ConsentDto>>();
                if (consentResponse == null)
                {
                    return new ServiceResponse<IEnumerable<ConsentDto>>() { StatusCode = 404, Response = consentResponse };
                }
                return new ServiceResponse<IEnumerable<ConsentDto>>() { StatusCode = 200, Response = consentResponse };
            }
            else
            {
                return new ServiceResponse<IEnumerable<ConsentDto>>() { StatusCode = (int)httpResponseMessage.StatusCode };
            }
        }

        public async Task<ServiceResponse<RoleDto>> GetRole(Guid roleId)
        {
            var httpClient = _httpClientFactory.CreateClient("Role");

            var httpResponseMessage = await httpClient.GetAsync(
                $"/role/{roleId}");

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var roleResponse = await httpResponseMessage.Content.ReadFromJsonAsync<RoleDto>();
                if (roleResponse == null)
                {
                    return new ServiceResponse<RoleDto>() { StatusCode = 404, Response = roleResponse };
                }
                return new ServiceResponse<RoleDto>() { StatusCode = 200, Response = roleResponse };
            }
            else
            {
                return new ServiceResponse<RoleDto>() { StatusCode = (int)httpResponseMessage.StatusCode };
            }
        }

        public async Task<ServiceResponse<RoleDefinitionDto>> GetRoleDefinition(Guid roleId)
        {
            var httpClient = _httpClientFactory.CreateClient("Role");

            var httpResponseMessage = await httpClient.GetAsync(
                $"/roleDefinition/{roleId}");

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var roleResponse = await httpResponseMessage.Content.ReadFromJsonAsync<RoleDefinitionDto>();
                if (roleResponse == null)
                {
                    return new ServiceResponse<RoleDefinitionDto>() { StatusCode = 404, Response = roleResponse };
                }
                return new ServiceResponse<RoleDefinitionDto>() { StatusCode = 200, Response = roleResponse };
            }
            else
            {
                return new ServiceResponse<RoleDefinitionDto>() { StatusCode = (int)httpResponseMessage.StatusCode };
            }
        }

        public async Task<ServiceResponse> MigrateRoleDefinitions(List<RoleDefinitionDto> roleDefinitions)
        {
            var httpClient = _httpClientFactory.CreateClient("Role");
            
            var request = new StringContent(JsonSerializer.Serialize(roleDefinitions), Encoding.UTF8, "application/json");
            var httpResponseMessage = await httpClient.PostAsync(
                "roleDefinition/upsert", request);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return new ServiceResponse() { StatusCode = 200 };
            }
            else
            {
                return new ServiceResponse() { StatusCode = (int) httpResponseMessage.StatusCode };
            }
        }
    }
}