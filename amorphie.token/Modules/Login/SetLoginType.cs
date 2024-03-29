using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using amorphie.token.core.Models.InternetBanking;
using amorphie.token.data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.EntityFrameworkCore;

namespace amorphie.token.Modules.Login
{
    public static class SetLoginType
    {
        [ApiExplorerSettings(IgnoreApi = true)]

        public static async Task<IResult> setLoginType(
        [FromBody] dynamic body,
        [FromServices] IUserService userService,
        [FromServices] IClientService clientService,
        [FromServices] IbDatabaseContext ibContext
        )
        {
            LoginResponse userInfo;

            var userInfoSerialized = body.GetProperty("userSerialized").ToString();

            userInfo = JsonSerializer.Deserialize<LoginResponse>(userInfoSerialized, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });


            var ibUserSerialized = body.GetProperty("ibUserSerialized").ToString();
            IBUser ibUser = JsonSerializer.Deserialize<IBUser>(ibUserSerialized);

            ClientResponse clientInfo;

            var clientInfoSerialized = body.GetProperty("clientSerialized").ToString();

            clientInfo = JsonSerializer.Deserialize<ClientResponse>(clientInfoSerialized, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            string securityImagePath = string.Empty;
            var securityImage = await ibContext.SecurityImage.Where(i => i.UserId == ibUser.Id)
                .OrderByDescending(i => i.CreatedAt).FirstOrDefaultAsync();

            if (securityImage != null)
            {
                var securityImageInfo = await ibContext.SecurityImageDefinition.Where(i => i.Id == securityImage.DefinitionId).FirstOrDefaultAsync();
                if (securityImageInfo != null)
                {
                    securityImagePath = securityImageInfo.ImagePath ?? string.Empty;
                }
            }

            var transitionName = body.GetProperty("LastTransition").ToString();
            var dataBody = body.GetProperty($"TRX-{transitionName}").GetProperty("Data");

            dynamic dataChanged = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(dataBody.ToString());

            dynamic targetObject = new System.Dynamic.ExpandoObject();

            targetObject.Data = dataChanged;

            var deviceId = body.GetProperty("Headers").GetProperty("xdeviceid").ToString();
            var installationId = body.GetProperty("Headers").GetProperty("xtokenid").ToString();
            ServiceResponse<object> response = await userService.CheckDevice(userInfo.Id, clientInfo.code ?? clientInfo.id!, deviceId, Guid.Parse(installationId));
            dynamic variables = new Dictionary<string, dynamic>();
            dataChanged.additionalData = new ExpandoObject();
            dataChanged.additionalData.phoneNumber = "0" + userInfo.MobilePhone.Prefix.ToString().Substring(0, 2) + "******" + userInfo.MobilePhone.Number.ToString().Substring(userInfo.MobilePhone.Number.Length - 2, 2);
            dataChanged.additionalData.securityImage = securityImagePath;
            targetObject.Data = dataChanged;
            targetObject.TriggeredBy = Guid.Parse(body.GetProperty($"TRX-{transitionName}").GetProperty("TriggeredBy").ToString());
            targetObject.TriggeredByBehalfOf = Guid.Parse(body.GetProperty($"TRX-{transitionName}").GetProperty("TriggeredByBehalfOf").ToString());
            variables.Add($"TRX{transitionName.ToString().Replace("-", "")}", targetObject);

            if (response.StatusCode == 200)
            {
                variables.Add("isSecondFactorRequired", false);
                return Results.Ok(variables);
            }

            if (response.StatusCode == 404)
            {
                variables.Add("isSecondFactorRequired", true);
                return Results.Ok(variables);
            }
            else
            {
                variables.Add("status", false);
                variables.Add("isSecondFactorRequired", response.Detail);
                return Results.Ok(variables);
            }
        }

    }
}