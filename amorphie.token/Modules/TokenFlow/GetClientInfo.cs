using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using amorphie.token.data;
using Elastic.Apm.Api;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace amorphie.token.Modules.TokenFlow
{
    public static class GetClientInfo
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        public static async Task<IResult> getClientInfo(
        [FromBody] dynamic body,
        [FromServices] IClientService clientService
        )
        {
            dynamic variables = new ExpandoObject();
            variables.ClientInfo = new ExpandoObject();

            string clientId = body.GetProperty("id").ToString();
            ServiceResponse<ClientResponse> clientResponse;
            if (Guid.TryParse(clientId, out Guid _))
            {
                clientResponse = await clientService.CheckClient(clientId);
            }
            else
            {
                clientResponse = await clientService.CheckClientByCode(clientId);
            }

            if (clientResponse.StatusCode != 200)
            {
                variables.ClientInfo.isValid = false;
                variables.ClientInfo.errorCode = clientResponse.StatusCode;
                variables.ClientInfo.errorMessage = clientResponse.Detail;
                return Results.Ok(variables);
            }
            var client = clientResponse.Response;

            variables.ClientInfo.isValid = true;
            variables.ClientInfo.data = client;

            return Results.Ok(variables);
        }
    }
}