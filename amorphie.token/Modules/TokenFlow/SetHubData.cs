using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using amorphie.token.core.Models.InternetBanking;
using amorphie.token.data;
using amorphie.token.Services.Consent;
using amorphie.token.Services.InternetBanking;
using Elastic.Apm.Api;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace amorphie.token.Modules.TokenFlow
{
  
    public static class SetHubData
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        public static async Task<IResult> setHubData(
        [FromBody] dynamic body,
        [FromServices] IConfiguration configuration
        )
        {
            var transitionName = body.GetProperty("LastTransition").ToString();

            var resultData = body.GetProperty("result");

            var dataBody = body.GetProperty($"TRX-{transitionName}").GetProperty("Data");

            dynamic dataChanged = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(dataBody.ToString());

            dynamic targetObject = new System.Dynamic.ExpandoObject();

            targetObject.Data = dataChanged;

            dynamic variables = new Dictionary<string, dynamic>();

            dataChanged.additionalData = new ExpandoObject();
            dataChanged.additionalData.result = resultData;
            targetObject.Data = dataChanged;
            targetObject.TriggeredBy = Guid.Parse(body.GetProperty($"TRX-{transitionName}").GetProperty("TriggeredBy").ToString());
            targetObject.TriggeredByBehalfOf = Guid.Parse(body.GetProperty($"TRX-{transitionName}").GetProperty("TriggeredByBehalfOf").ToString());
            variables.Add($"TRX{transitionName.ToString().Replace("-", "")}", targetObject);

            return Results.Ok(variables);
        }
    }
}