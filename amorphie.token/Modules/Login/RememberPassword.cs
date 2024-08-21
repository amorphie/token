using amorphie.token.core;
using Microsoft.AspNetCore.Mvc;

namespace amorphie.token;


public class SwitchMethodRequest
{

    public string UserName { get; set; }
    public string From { get; set; }
    public bool HasQuestion { get; set; }
    public bool HasCard { get; set; }
    public bool HasNfc { get; set; }
    public bool HasVideoCall { get; set; }
    public bool HasNewIdentityCard { get; set; }
}

public class SwitchMethodResponse
{

    public string From { get; set; }
    public bool HasQuestion { get; set; }
    public bool HasCard { get; set; }
    public bool HasNfc { get; set; }
    public bool HasVideoCall { get; set; }
    public bool HasNewIdentityCard { get; set; }

    public bool IsValidOsVersion { get; set; }
    public Dictionary<string, string> SwitchMethodErrorMessage { get; set; }
}



public class RememberPassword
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public static async Task<IResult> SwitchMethod(
        [FromHeader(Name = "xdeviceversion")] string? version,
        [FromHeader(Name = "xdeployment")] string? deployment,
        [FromBody] SwitchMethodRequest body,
        [FromServices] IEkycProvider ekycProvider
    )
    {
        var message = new Dictionary<string, string>();

        if (!body.HasNewIdentityCard)
        {
            message = ErrorMessages.OldIdentityCard;
        }
        if (!body.HasNfc && !body.HasVideoCall)
        {
            message = ErrorMessages.HasNotNfcAndNewIdentityCard;
        }
        version = version is null ?"0.0":version;
        
        var isValidOsVersion = ValidateOsVersion(deployment, float.Parse(version));

        var response = new SwitchMethodResponse
        {
            From = body.From,
            HasQuestion = body.HasQuestion,
            HasCard = body.HasCard,
            HasNfc = body.HasNfc,
            HasVideoCall = body.HasVideoCall,
            HasNewIdentityCard = body.HasNewIdentityCard,
            SwitchMethodErrorMessage = message,
            IsValidOsVersion = isValidOsVersion
        };


        return Results.Ok(response);
    }


    private static bool ValidateOsVersion(string? xDeployment, float xDeviceVersion)
    {
        // string xDeployment = body.GetProperty("xdeployment").ToString();
        // float.TryParse(body.GetProperty("xdeviceversion").ToString(), out float xDeviceVersion);

        return (xDeployment == "iOS" && xDeviceVersion >= 12) || (xDeployment == "Android" && xDeviceVersion >= 7);

    }
}
