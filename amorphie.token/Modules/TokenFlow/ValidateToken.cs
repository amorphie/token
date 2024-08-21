
using System.Dynamic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using amorphie.token.data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace amorphie.token.Modules.TokenFlow
{
    public class ValidateTokenRequest
    {
        public ClientResponse client{get;set;}
        public string token{get;set;}
    }

    public static class ValidateToken
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        public static async Task<IResult> validateToken(
        [FromBody] ValidateTokenRequest validateTokenRequest
        )
        {
            dynamic variables = new ExpandoObject();
            variables.Token = new ExpandoObject();
            
            JwtSecurityToken? tokenValidated;
            var secretKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(validateTokenRequest.client.jwtSalt!));
            if (!JwtHelper.ValidateToken(validateTokenRequest.token!, "BurganIam", validateTokenRequest.client.returnuri, secretKey, out tokenValidated))
            {
                variables.Token.isValid = false;
            }
            

            return Results.Ok(variables);
        }
    }
}