
using System.Dynamic;
using System.IdentityModel.Tokens.Jwt;
using amorphie.token.data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace amorphie.token.Modules.TokenFlow
{
    public static class GetRefreshTokenInfo
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        public static async Task<IResult> getRefreshTokenInfo(
        [FromBody] dynamic body,
        [FromServices] DatabaseContext databaseContext
        )
        {
            dynamic variables = new ExpandoObject();
            variables.RefreshToken = new ExpandoObject();
            string refreshToken = body.GetProperty("refresh_token").ToString();
            var refreshTokenJti = JwtHelper.GetClaim(refreshToken, "jti");
            if (refreshTokenJti == null)
            {
                variables.RefreshToken.isExist = false;
                return Results.Ok(variables);
            }

            var refreshTokenInfo = await databaseContext.Tokens.FirstOrDefaultAsync(t =>
            t.Id == Guid.Parse(refreshTokenJti!) && t.TokenType == TokenType.RefreshToken);
            if (refreshTokenInfo == null)
            {
                variables.RefreshToken.isExist = false;
                return Results.Ok(variables);
            }
            variables.RefreshToken.isExist = true;

            var relatedToken = await databaseContext.Tokens.FirstOrDefaultAsync(t =>
            t.Id == refreshTokenInfo!.RelatedTokenId && t.TokenType == TokenType.AccessToken);
            if (relatedToken == null)
            {
                variables.RefreshToken.isRelatedTokenExist = false;
                return Results.Ok(variables);
            }
            variables.RefreshToken.isRelatedTokenExist = true;

            variables.RefreshToken.clientId = refreshTokenInfo!.ClientId;

            if(refreshTokenInfo!.UserId is not null)
            {
                variables.RefreshToken.hasUser = true;
                variables.RefreshToken.userId = refreshTokenInfo.UserId;
            }
            else
            {
                variables.RefreshToken.hasUser = false;
            }
            
            if(refreshTokenInfo!.ConsentId is not null)
            {
                variables.RefreshToken.hasObConsent = true;
                variables.RefreshToken.obConsentId = true;
            }
            else
            {
                variables.RefreshToken.hasObConsent = false;
            }

            variables.RefreshToken.data = refreshTokenInfo;

            return Results.Ok(variables);
        }
    }
}