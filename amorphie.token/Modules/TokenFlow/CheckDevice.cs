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
    public class CheckDeviceRequest
    {
        public string clientId{get;set;}
        public string reference{get;set;}
    }

    public static class CheckDevice
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        public static async Task<IResult> checkDevice(
        [FromBody] CheckDeviceRequest checkDeviceRequest,
        [FromServices] IUserService userService
        )
        {
            dynamic variables = new ExpandoObject();
            variables.DeviceInfo = new ExpandoObject();

            var device = await userService.CheckDevice(checkDeviceRequest.reference, checkDeviceRequest.clientId);
            if(device.StatusCode == 200)
            {
                variables.DeviceInfo.data = device.Response;
                variables.DeviceInfo.isExist = true;
            }
            else
            {
                variables.DeviceInfo.isExist = false;
            }
            
            return Results.Ok(variables);
        }
    }
}