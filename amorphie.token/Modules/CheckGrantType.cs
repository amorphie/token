
using System.Dynamic;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace amorphie.token.Modules;

public static class CheckGrantTypes
{
    public static void MapCheckGrantTypesControlEndpoints(this WebApplication app)
    {
        app.MapPost("/check-grant-type", checkGrantTypes)
        .Produces(StatusCodes.Status200OK);

        static async Task<IResult> checkGrantTypes(
        [FromBody] dynamic body,
        [FromServices] IAuthorizationService authorizationService
        )
        {
            var requestBodySerialized = body.GetProperty("body").ToString();
            
            TokenRequest requestBody = JsonSerializer.Deserialize<TokenRequest>(requestBodySerialized,new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var clientInfoSerialized = body.GetProperty("clientSerialized").ToString();
            
            ClientResponse clientInfo = JsonSerializer.Deserialize<ClientResponse>(clientInfoSerialized,new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            if(clientInfo.allowedgranttypes == null || !clientInfo.allowedgranttypes.Any(g => g.GrantType == requestBody.grant_type))
            {
                dynamic variables = new ExpandoObject();
                variables.status = false;
                variables.message = "Client Has No Authorize To Use Requested Grant Type";
                return Results.Ok(variables);
            }
            else
            {
                dynamic variables = new ExpandoObject();
                variables.status = true;
                return Results.Ok(variables);
            }
        }

    }
}