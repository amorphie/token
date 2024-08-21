using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using amorphie.token.core.Models.InternetBanking;
using amorphie.token.core.Models.Profile;
using amorphie.token.data;
using amorphie.token.Services.Consent;
using amorphie.token.Services.InternetBanking;
using Elastic.Apm.Api;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace amorphie.token.Modules.TokenFlow
{
    public class GenerateAuthCodeRequest
    {
        public AuthorizationRequestBody AuthorizationRequest { get; set; }
        public SimpleProfileResponse? Profile { get; set; }
        public LoginResponse? User { get; set; }
    }

    public static class GenerateAuthCode
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        public static async Task<IResult> generateAuthCode(
        [FromBody] GenerateAuthCodeRequest generateAuthCodeRequest,
        [FromServices] IAuthorizationService authorizationService
        )
        {
            dynamic variables = new ExpandoObject();

            var authCode = await authorizationService.Authorize(new AuthorizationServiceRequest
            {
                ClientId = generateAuthCodeRequest.AuthorizationRequest.ClientId,
                CodeChallange = generateAuthCodeRequest.AuthorizationRequest.CodeChallange,
                CodeChallangeMethod = generateAuthCodeRequest.AuthorizationRequest.CodeChallangeMethod,
                Nonce = generateAuthCodeRequest.AuthorizationRequest.Nonce,
                Scope = generateAuthCodeRequest.AuthorizationRequest.Scope,
                RedirectUri = generateAuthCodeRequest.AuthorizationRequest.RedirectUri,
                Profile = generateAuthCodeRequest.Profile,
                ResponseType = generateAuthCodeRequest.AuthorizationRequest.ResponseType,
                User = generateAuthCodeRequest.User,
                State = generateAuthCodeRequest.AuthorizationRequest.State,
            });

            if(authCode.StatusCode != 200)
            {
                //TODO
            }
            
            variables.AuthCodeInfo = authCode.Response;
            
            return Results.Ok(variables);
        }
    }
}