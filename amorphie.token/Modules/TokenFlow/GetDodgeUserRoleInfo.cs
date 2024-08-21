using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using amorphie.token.data;
using amorphie.token.Services.Consent;
using amorphie.token.Services.InternetBanking;
using Elastic.Apm.Api;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace amorphie.token.Modules.TokenFlow
{
    public static class GetDodgeUserRoleInfo
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        public static async Task<IResult> getDodgeUserRoleInfo(
        [FromBody] dynamic body,
        [FromServices] IbDatabaseContext ibDatabaseContext
        )
        {
            dynamic variables = new ExpandoObject();
            variables.DodgeRoleInfo = new ExpandoObject();

            Guid userId = Guid.Parse(body.GetProperty("id").ToString());
            var role = await ibDatabaseContext.Role.Where(r => r.UserId.Equals(userId) && r.Channel.Equals(10) && r.Status.Equals(10)).OrderByDescending(r => r.CreatedAt).FirstOrDefaultAsync();
            if(role is {} && (role.ExpireDate ?? DateTime.MaxValue) > DateTime.Now)
            {
                var roleDefinition = await ibDatabaseContext.RoleDefinition.FirstOrDefaultAsync(d => d.Id.Equals(role.DefinitionId) && d.IsActive);
                if(roleDefinition is {})
                {
                    variables.DodgeRoleInfo.roleKey = roleDefinition.Key;
                }
            }
            else
            {
                variables.DodgeRoleInfo.roleKey = 0;
            }            

            return Results.Ok(variables);
        }
    }
}