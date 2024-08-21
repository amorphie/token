using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using amorphie.token.data;
using amorphie.token.Services.Consent;
using Elastic.Apm.Api;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace amorphie.token.Modules.TokenFlow
{
    public static class GetUserInfo
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        public static async Task<IResult> getUserInfo(
        [FromBody] dynamic body,
        [FromServices] IUserService userService
        )
        {
            dynamic variables = new ExpandoObject();
            variables.UserInfo = new ExpandoObject();
            
            string userKey = body.GetProperty("id").ToString();
            
            ServiceResponse<LoginResponse> userResponse;
            if(Guid.TryParse(userKey, out Guid userId))
            {
                userResponse = await userService.GetUserById(userId);
            }
            else
            {
                userResponse = await userService.GetUserByReference(userKey);
            }
            

            if (userResponse.StatusCode != 200)
            {
                variables.UserInfo.isExist = false;
                variables.UserInfo.errorCode = userResponse.StatusCode;
                variables.UserInfo.errorMessage = userResponse.Detail;
                return Results.Ok(variables);
            }
            var user = userResponse.Response;
            
            variables.UserInfo.isExist = true;
            variables.UserInfo.data = user;

            return Results.Ok(variables);
        }
    }
}