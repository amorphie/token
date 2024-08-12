using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using amorphie.token.core.Extensions;
using amorphie.token.core.Models.Profile;
using amorphie.token.data;
using amorphie.token.Modules.OtpProcess;
using amorphie.token.Services.TransactionHandler;
using Microsoft.AspNetCore.Mvc;

namespace amorphie.token.Modules.Login
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public static class GenerateTokens
    {
        public static async Task<IResult> generateTokens(
        [FromBody] dynamic body,
        [FromServices] ITokenService tokenService,
        [FromServices] ITransactionService transactionService,
        [FromServices] IUserService userService
        )
        {
            var transitionName = body.GetProperty("LastTransition").ToString();

            var dataBody = body.GetProperty($"TRX-{transitionName}").GetProperty("Data");

            dynamic dataChanged = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(dataBody.ToString());

            dynamic targetObject = new System.Dynamic.ExpandoObject();

            targetObject.Data = dataChanged;
        
            var requestBodySerialized = body.GetProperty("requestBody").ToString();
            TokenRequest requestBody = JsonSerializer.Deserialize<TokenRequest>(requestBodySerialized, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var clientInfoSerialized = body.GetProperty("clientSerialized").ToString();

            ClientResponse clientInfo = JsonSerializer.Deserialize<ClientResponse>(clientInfoSerialized, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var userInfoSerialized = body.GetProperty("userSerialized").ToString();

            LoginResponse userInfo = JsonSerializer.Deserialize<LoginResponse>(userInfoSerialized, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            SimpleProfileResponse? profile;
            try
            {
                var profileSerialized = body.GetProperty("userInfoSerialized").ToString();

                profile = JsonSerializer.Deserialize<SimpleProfileResponse>(profileSerialized, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (System.Exception)
            {
                profile = null;
            }
            

            string xforwardedfor = body.GetProperty("Headers").GetProperty("xforwardedfor").ToString();
            var ipAddress = xforwardedfor.Split(",")[0].Trim();
            try
            {
                string UserRoleKey = body.GetProperty("UserRoleKey").ToString();
                transactionService.RoleKey = Convert.ToInt32(UserRoleKey);
            }
            catch (Exception)
            {
                transactionService.RoleKey = 10;
            }
            
            transactionService.IpAddress = ipAddress;
            var deviceId = body.GetProperty("Headers").GetProperty("xdeviceid").ToString();
            var installationId = body.GetProperty("Headers").GetProperty("xtokenid").ToString();
            var platform = body.GetProperty("Headers").GetProperty("xdeployment").ToString();
            string model;
            string version;
            try
            {
                model = body.GetProperty("Headers").GetProperty("xdeviceinfo").ToString();
                version = body.GetProperty("Headers").GetProperty("xdeviceversion").ToString();
            }
            catch (Exception ex)
            {
                model = string.Empty;
                version = string.Empty;
            }
            
            ServiceResponse<TokenResponse> result = await tokenService.GenerateTokenWithPasswordFromWorkflow(requestBody.MapTo<GenerateTokenRequest>(), clientInfo, userInfo, profile, deviceId);

            if (result.StatusCode == 200)
            {

                //For Default User
                if (userInfo.Reference != "99999999998")
                {
                    await userService.SaveDevice(new UserSaveMobileDeviceDto()
                    {
                        DeviceId = deviceId,
                        InstallationId = Guid.Parse(installationId),
                        DeviceModel = model,
                        DevicePlatform = platform,
                        ClientId = clientInfo.code ?? clientInfo.id,
                        UserId = userInfo.Id,
                        DeviceVersion = version
                    });
                }


                dataChanged.additionalData = result.Response;
                targetObject.Data = dataChanged;
                targetObject.TriggeredBy = Guid.Parse(body.GetProperty($"TRX-{transitionName}").GetProperty("TriggeredBy").ToString());
                targetObject.TriggeredByBehalfOf = Guid.Parse(body.GetProperty($"TRX-{transitionName}").GetProperty("TriggeredByBehalfOf").ToString());
                dynamic variables = new Dictionary<string, dynamic>();
                variables.Add("status", true);
                variables.Add($"TRX{transitionName.ToString().Replace("-", "")}", targetObject);
                transactionService.Logon.LogonStatus = LogonStatus.Completed;
                return Results.Ok(variables);
            }
            else
            {
                dynamic variables = new ExpandoObject();
                variables.status = false;
                variables.tokenResponse = result.Detail;
                return Results.Ok(variables);
            }

        }
    }
}