using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using amorphie.token.core.Extensions;
using amorphie.token.core.Models.Consent;
using amorphie.token.core.Models.InternetBanking;
using amorphie.token.core.Models.Profile;
using amorphie.token.data;
using Elastic.Apm.Api;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace amorphie.token.Modules.TokenFlow
{
    public class GenerateTokenRequestWithRefreshTokenRequest
    {
        public TokenInfo refresh_token_info{get;set;}
        public LoginResponse user{get;set;}
        public IBUser? dodge_user{get;set;}
        public ConsentResponse? consent{get;set;}
        public int? dodge_role{get;set;}
        public SimpleProfileResponse? profile{get;set;}
        public ClientResponse client{get;set;}

    }

    public static class GenerateTokensWithRefreshToken
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        public static async Task<IResult> generateTokensWithRefreshToken(
        [FromBody] GenerateTokenRequestWithRefreshTokenRequest generateTokenRequestWithRefreshTokenRequest,
        [FromServices] ITokenService tokenService
        )
        {
            dynamic variables = new ExpandoObject();
            variables.TokenInfo = new ExpandoObject();

            var response = await tokenService.GenerateTokenWithRefreshTokenFromWorkflowAsync
            (
                refreshTokenInfo: generateTokenRequestWithRefreshTokenRequest.refresh_token_info,
                user: generateTokenRequestWithRefreshTokenRequest.user,
                profile: generateTokenRequestWithRefreshTokenRequest.profile,
                client: generateTokenRequestWithRefreshTokenRequest.client,
                dodgeRoleKey: generateTokenRequestWithRefreshTokenRequest.dodge_role,
                consent: generateTokenRequestWithRefreshTokenRequest.consent,
                dodgeUser: generateTokenRequestWithRefreshTokenRequest.dodge_user
            );
            
            variables.TokenInfo.data = response.Response;
            variables.TokenInfo.status = response.StatusCode;
            variables.TokenInfo.message = response.Detail;

            return Results.Ok(variables);
        }
    }
}