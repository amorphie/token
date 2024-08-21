using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using amorphie.token.data;
using amorphie.token.Services.Consent;
using amorphie.token.Services.Profile;
using Elastic.Apm.Api;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace amorphie.token.Modules.TokenFlow
{
    public static class GetSimpleProfileInfo
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        public static async Task<IResult> getSimpleProfileInfo(
        [FromBody] dynamic body,
        [FromServices] IProfileService profileService
        )
        {
            dynamic variables = new ExpandoObject();
            variables.ProfileInfo = new ExpandoObject();

            string reference = body.GetProperty("reference").ToString();
            var profileResponse = await profileService.GetCustomerSimpleProfile(reference);

            if (profileResponse.StatusCode != 200)
            {
                variables.ProfileInfo.isExist = false;
                variables.ProfileInfo.errorCode = profileResponse.StatusCode;
                variables.ProfileInfo.errorMessage = profileResponse.Detail;
                return Results.Ok(variables);
            }
            var profile = profileResponse.Response;
            
            variables.ProfileInfo.isExist = true;
            variables.ProfileInfo.data = profile;

            return Results.Ok(variables);
        }
    }
}