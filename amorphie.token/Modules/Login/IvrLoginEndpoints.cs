using Microsoft.AspNetCore.Mvc;
using amorphie.token.core.Models.InternetBanking;
using amorphie.token.core.Models.Profile;
using amorphie.token.data;
using amorphie.token.Services.Cardion;
using amorphie.token.Services.InternetBanking;
using amorphie.token.Services.Login;
using amorphie.token.Services.Profile;
using Microsoft.EntityFrameworkCore;

namespace amorphie.token;

#region Records

public class Response<T>
{
    public int Code { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
}

public record GetCustomerChannelsInput
{
    public string CallUuid { get; set; }
    public string CitizenshipNo { get; set; }
}

public record GetCustomerChannelsOutput
{
    public string CallUuid { get; set; }
    public string CitizenshipNo { get; set; }
    public bool Card { get; set; }
    public bool Ibank { get; set; }
}

public record LoginTypeInput
{
    public string CallUuid { get; set; }
    public string CitizenshipNo { get; set; }
    public string PinCode { get; set; }
}

public record LoginTypeOutput
{
    public string OtpType { get; set; }
    public string PhoneNumber { get; set; }
}

public record GenerateOtpInput
{
    public string CallUuid { get; set; }
    public string CitizenshipNo { get; set; }
}

public record GenerateOtpOutput
{
    public string OtpCode { get; set; }
}

public record CheckSmsOtpInput
{
    public string CallUuid { get; set; }
    public string CitizenshipNo { get; set; }
    public string OtpCode { get; set; }
}

public record CheckOtpOutput
{
    public bool Valid { get; set; }
}

public class AuthorizationInput
{
    public string? ResponseType { get; set; }
    public string? ClientId { get; set; }
    public string[]? Scope { get; set; }
    public string? State { get; set; }
    public string UserName { get; set; }
}

public class AuthorizationOutput
{
    public string Code { get; set; }
}

#endregion

public static class IvrLoginEndpoints
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public static async Task<IResult> GetCustomerChannelsAsync(
        [FromBody] GetCustomerChannelsInput input,
        [FromServices] IInternetBankingUserService ibUserService,
        [FromServices] IbDatabaseContext ibContext,
        [FromServices] ICardionService cardionService,
        IConfiguration configuration
    )
    {
        var response = new Response<GetCustomerChannelsOutput>();
        response.Data = new GetCustomerChannelsOutput();
        response.Data.CitizenshipNo = input.CitizenshipNo;
        response.Data.CallUuid = input.CallUuid;
        if (input.CallUuid.Length != 32 || string.IsNullOrEmpty(input.CitizenshipNo))
        {
            response.Code = 599;
            response.Message = "request not in proper format";
            return Results.Json(response, statusCode: response.Code);
        }

        var ibUserResponse = await IbUserCheckAsync(ibContext, ibUserService, input.CitizenshipNo);
        var cardResponse = await CardUserCheckAsync(cardionService, input.CitizenshipNo);
        response.Code = 200;
        response.Message = "Success";
        if (ibUserResponse.Code != 200 && cardResponse.Code != 200)
        {
            response.Code = 598;
            response.Message = "Both Passive";
            response.Data.Ibank = false;
            response.Data.Card = false;
            return Results.Json(response, statusCode: response.Code);
        }

        response.Data.Ibank = ibUserResponse.Code == 200;
        response.Data.Card = cardResponse.Code == 200;

        return Results.Ok(response);
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    public static async Task<IResult> LoginTypeAsync(
        [FromBody] LoginTypeInput input,
        [FromServices] IProfileService profileService,
        [FromServices] IUserService userService,
        [FromServices] ICardionService cardionService,
        [FromServices] ILoginService loginService,
        IConfiguration configuration
    )
    {
        var response = new Response<LoginTypeOutput>();
        var migrateResult = await loginService.MigrateDodgeUserToAmorphie(input.CitizenshipNo);
        if (migrateResult.StatusCode != 200)
        {
            response.Code = migrateResult.StatusCode;
            response.Message = migrateResult.Detail;
            return Results.Json(response, statusCode: response.Code);
        }
        
        var userInfoResult = await profileService.GetCustomerSimpleProfile(input.CitizenshipNo);
        if (userInfoResult.StatusCode != 200)
        {
            response.Code = 595;
            response.Message = "User Profile not found";
            return Results.Json(response, statusCode: response.Code);
        }

        var userInfo = userInfoResult.Response;
        if (userInfo!.data!.profile!.Equals("customer") || !userInfo!.data!.profile!.status!.Equals("active"))
        {
            response.Code = 595;
            response.Message = "User is not active";
            return Results.Json(response, statusCode: response.Code);
        }

        var pinCheckResponse = await CardPinCheckAsync(cardionService, input.CitizenshipNo, input.PinCode);

        if (pinCheckResponse.Code == 200)
        {
            response.Code = 595;
            response.Message = "User is not active";
            return Results.Json(response, statusCode: response.Code);
        }

        var mobileClientCodes = configuration["IVR:ClientCodes"]?.Split(',');
        if (mobileClientCodes != null)
        {
            var userDeviceResponse = await GetDevice(userService, input.CitizenshipNo, mobileClientCodes);
            if (userDeviceResponse != null)
            {
                response.Code = 200;
                response.Message = "Success";
                response.Data = new LoginTypeOutput()
                {
                    OtpType = "MobilePush"
                };
                return Results.Ok(response);
            }
        }

        var mobilePhone = GetMobilePhone(userInfo);
        if (mobilePhone == null)
        {
            response.Code = 595;
            response.Message = "Device and mobile are not active";
            return Results.Json(response, statusCode: response.Code);
        }

        response.Code = 200;
        response.Message = "Success";
        response.Data = new LoginTypeOutput()
        {
            OtpType = "SmsOtp",
            PhoneNumber = $"{mobilePhone.countryCode}{mobilePhone.prefix}{mobilePhone.number}"
        };
        return Results.Ok(response);
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    public static async Task<IResult> GenerateOtpAsync(
        [FromBody] GenerateOtpInput input,
        DaprClient daprClient,
        IConfiguration configuration
    )
    {
        var response = new Response<GenerateOtpOutput>();

        var code = GenerateOtpCode();
        await daprClient.SaveStateAsync(configuration["DAPR_STATE_STORE_NAME"], $"{input.CallUuid}_IvrLogin_Otp_Code",
            code,
            metadata: new Dictionary<string, string> { { "ttlInSeconds", "180" } });

        response.Code = 200;
        response.Message = "Success";
        response.Data = new GenerateOtpOutput()
        {
            OtpCode = code
        };
        return Results.Ok(response);
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    public static async Task<IResult> CheckSmsOtpAsync(
        [FromBody] CheckSmsOtpInput input,
        DaprClient daprClient,
        IConfiguration configuration
    )
    {
        var response = new Response<CheckOtpOutput>();
        response.Data = new CheckOtpOutput();
        var generatedCode = await daprClient.GetStateAsync<string?>(configuration["DAPR_STATE_STORE_NAME"],
            $"{input.CallUuid}_IvrLogin_Otp_Code");
        if (generatedCode == null)
        {
            response.Code = 595;
            response.Message = "Password verification code has been expired, send again.";
            return Results.Json(response, statusCode: response.Code);
        }

        if (input.OtpCode == generatedCode)
        {
            response.Code = 200;
            response.Message = "Success";
            response.Data.Valid = true;
            return Results.Ok(response);
        }

        response.Code = 597;
        response.Message = "You entered the password verification code incorrectly, try again.";
        return Results.Json(response, statusCode: response.Code);
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    public static async Task<IResult> AuthorizeAsync(
        [FromBody] AuthorizationInput input,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IProfileService profileService,
        [FromServices] IUserService userService,
        [FromServices ]ILoginService loginService,
        IConfiguration configuration
    )
    {
        var response = new Response<AuthorizationOutput>();
        response.Data = new AuthorizationOutput();
        var userResponse = await userService.GetUserByReference(input.UserName);
        var userInfoResult = await profileService.GetCustomerSimpleProfile(input.UserName);
        var authorize = await authorizationService.Authorize(new AuthorizationServiceRequest
        {
            ClientId = input.ClientId,
            ResponseType = input.ResponseType,
            Scope = input.Scope,
            State = input.State,
            User = userResponse.Response,
            Profile = userInfoResult.Response
        });

        if (authorize.StatusCode == 200)
        {
            response.Code = 200;
            response.Message = "Success";
            response.Data.Code = authorize!.Response!.Code;

            await authorizationService.AssignUserToAuthorizationCode(userResponse.Response, authorize.Response.Code,
                userInfoResult.Response);
            
            return Results.Ok(response);
        }

        response.Code = authorize.StatusCode;
        response.Message = authorize.Detail;
        return Results.Json(response, statusCode: response.Code);
    }

    #region Private Methods

    private static string GenerateOtpCode()
    {
        var rand = new Random();
        var code = String.Empty;
        for (int i = 0; i < 6; i++)
        {
            code += rand.Next(10);
        }

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (env != null && !env.Equals("Prod"))
            code = "123456";

        return code;
    }

    private static async Task<Response<IBUser>> IbUserCheckAsync(
        IbDatabaseContext ibContext,
        IInternetBankingUserService ibUserService,
        string citizenshipNo)
    {
        var response = new Response<IBUser>();
        try
        {
            var userResponse = await ibUserService.GetUser(citizenshipNo);

            if (userResponse.StatusCode != 200)
            {
                response.Code = 595;
                response.Message = "content is null";
                return response;
            }

            var userStatus = await ibContext.Status
                .Where(s => s.UserId == userResponse.Response!.Id && (!s.State.HasValue || s.State.Value == 10))
                .OrderByDescending(s => s.CreatedAt).FirstOrDefaultAsync();
            if (userStatus?.Type == 30 || userStatus?.Type == 40)
            {
                response.Code = 593;
                response.Message = "IsUserStatusActive null";
                return response;
            }

            var passwordResponse = await ibUserService.GetPassword(userResponse.Response!.Id);
            if (passwordResponse.StatusCode != 200)
            {
                response.Code = 593;
                response.Message = "IsUserStatusActive null";
                return response;
            }

            response.Code = 200;
            response.Message = "Success";
            return response;
        }
        catch (Exception e)
        {
            response.Code = 599;
            response.Message = "General error";
            return response;
        }
    }

    private static async Task<Response<bool>> CardUserCheckAsync(ICardionService cardionService, string citizenshipNo)
    {
        try
        {
            var checkResponse = await cardionService.GetCardListAsync(citizenshipNo);
            var state = checkResponse.StatusCode == 200 && checkResponse.Result.Any();
            return new Response<bool>()
            {
                Code = state ? 200 : 593,
                Message = state ? "Success" : "IsUserStatusActive null",
                Data = checkResponse.StatusCode == 200 && checkResponse.Result.Any()
            };
        }
        catch (Exception e)
        {
            return new Response<bool>()
            {
                Code = 599,
                Message = "General error",
                Data = false
            };
        }
    }

    private static async Task<Response<bool>> CardPinCheckAsync(ICardionService cardionService, string citizenshipNo,
        string pinCode)
    {
        try
        {
            var checkResponse = await cardionService.CardValidatePinAsync(citizenshipNo,
                new CardionCardValidatePinRequest()
                {
                    Pin = pinCode
                });
            var state = checkResponse.StatusCode == 200;
            return new Response<bool>()
            {
                Code = state ? 200 : 593,
                Message = state ? "Success" : "Pin invalid",
                Data = checkResponse.StatusCode == 200
            };
        }
        catch (Exception e)
        {
            return new Response<bool>()
            {
                Code = 599,
                Message = "General error",
                Data = false
            };
        }
    }

    private static SimpleProfilePhone? GetMobilePhone(SimpleProfileResponse userInfo)
    {
        var mobilePhoneCount = userInfo!.data!.phones!.Count(p => p.type!.Equals("mobile"));
        if (mobilePhoneCount != 1)
        {
            return null;
        }

        var mobilePhone = userInfo!.data!.phones!.FirstOrDefault(p => p.type!.Equals("mobile"));
        if (string.IsNullOrWhiteSpace(mobilePhone!.prefix) || string.IsNullOrWhiteSpace(mobilePhone!.number))
        {
            return null;
        }

        return mobilePhone;
    }

    private static async Task<GetPublicDeviceDto?> GetDevice(IUserService userService, string citizenshipNo,
        string[] clientCodes)
    {
        foreach (var clientCode in clientCodes)
        {
            var userDeviceResponse = await userService.GetPublicDevice(clientCode, citizenshipNo);
            if (userDeviceResponse.StatusCode == 200 && userDeviceResponse.Response?.Status == 10)
            {
                return userDeviceResponse.Response;
            }
        }

        return null;
    }

    #endregion
}
