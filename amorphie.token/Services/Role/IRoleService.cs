using System;

using amorphie.token.core.Models.Role;

namespace amorphie.token.Services.Role
{
    public interface IRoleService
    {
        public Task<ServiceResponse<IEnumerable<ConsentDto>>> GetConsents(string clientCode,string reference);
        public Task<ServiceResponse<RoleDto>> GetRole(Guid roleId);
        public Task<ServiceResponse<RoleDefinitionDto>> GetRoleDefinition(Guid roleId);
        public Task<ServiceResponse> MigrateRoleDefinitions(List<RoleDefinitionDto> roleDefinitions);
    }
}