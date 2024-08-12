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
    public Dictionary<string,string> SwitchMethodErrorMessage { get; set; }
}



public class RememberPassword
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public static async Task<IResult> SwitchMethod(
        [FromBody] SwitchMethodRequest body,
        [FromServices] IEkycProvider ekycProvider
    )
    {
        var message = new Dictionary<string, string>();

        if(!body.HasNewIdentityCard){
            message = ErrorMessages.OldIdentityCard;
        }
        if(!body.HasNfc && !body.HasVideoCall){
            message = ErrorMessages.HasNotNfcAndNewIdentityCard;
        }

        var response = new SwitchMethodResponse
        {
            From = body.From,
            HasQuestion = body.HasQuestion,
            HasCard = body.HasCard,
            HasNfc = body.HasNfc,
            HasVideoCall = body.HasVideoCall,
            HasNewIdentityCard = body.HasNewIdentityCard,
            SwitchMethodErrorMessage = message
        };

        //Default Route
        // var response = new Response<SwitchMethodResponse>()
        // {
        //     Code = 511,
        //     Message = "Not Selectable Method",
        //     Data = new SwitchMethodResponse
        //     {
        //         Route = null,
        //         SwitchMethodErrorMessage = null
        //     }
        // };



        // if (body.From == "question" && body.HasCard)
        // {
        //     response.Code = 200;
        //     response.Message = "Success";
        //     response.Data = new SwitchMethodResponse
        //     {
        //         Route = "card",
        //         SwitchMethodErrorMessage = null
        //     };
            
        // }

        // if(body.From=="card" && body.HasNfc && body.HasNewIdentityCard){
        //     response.Code = 200;
        //     response.Message = "Success";
        //     response.Data = new SwitchMethodResponse
        //     {
        //         Route = "identity",
        //         SwitchMethodErrorMessage = null
        //     };
           
        // }


        // if ((body.From == "question" && body.HasCard == false) || (body.From == "card"))
        // {
        //    if()
        // }



        return Results.Ok(response);
    }

}
