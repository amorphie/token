using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using amorphie.token.core.Models.Role;

namespace amorphie.token.Services.Role
{
    public class RoleService : ServiceBase, IRoleService
    {
        private readonly DaprClient _daprClient;
        public RoleService(ILogger<RoleService> logger, IConfiguration configuration, DaprClient daprClient) : base(logger, configuration)
        {
            _daprClient = daprClient;
        }

        public async Task<ServiceResponse<IEnumerable<ConsentDto>>> GetConsents(string clientCode, string reference)
        {
            try
            {
                var consents = await _daprClient.InvokeMethodAsync<IEnumerable<ConsentDto>>(HttpMethod.Get, Configuration["ConsentServiceAppName"], $"consent/user/{reference}");

                return new ServiceResponse<IEnumerable<ConsentDto>>()
                {
                    StatusCode = 200,
                    Detail = "",
                    Response = consents
                };
            }
            catch (InvocationException ex)
            {
                return new ServiceResponse<IEnumerable<ConsentDto>>()
                {
                    StatusCode = (int)ex.Response.StatusCode,
                    Detail = await ex.Response.Content.ReadAsStringAsync()
                };
            }
            catch (System.Exception ex)
            {
                return new ServiceResponse<IEnumerable<ConsentDto>>()
                {
                    StatusCode = 500,
                    Detail = ex.ToString()
                };
            }
        }

        public async Task<ServiceResponse<RoleDefinitionDto>> GetRoleDefinition(Guid roleId)
        {
            try
            {
                var roleDefinition = await _daprClient.InvokeMethodAsync<RoleDefinitionDto>(HttpMethod.Get, Configuration["RoleServiceAppName"], $"role/{roleId}");

                return new ServiceResponse<RoleDefinitionDto>()
                {
                    StatusCode = 200,
                    Detail = "",
                    Response = roleDefinition
                };
            }
            catch (InvocationException ex)
            {
                return new ServiceResponse<RoleDefinitionDto>()
                {
                    StatusCode = (int)ex.Response.StatusCode,
                    Detail = await ex.Response.Content.ReadAsStringAsync()
                };
            }
            catch (System.Exception ex)
            {
                return new ServiceResponse<RoleDefinitionDto>()
                {
                    StatusCode = 500,
                    Detail = ex.ToString()
                };
            }
        }
    }
}