
using System.Dynamic;
using System.Text.Json;
using amorphie.token.core.Models.InternetBanking;
using amorphie.token.data;
using amorphie.token.Services.Card;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace amorphie.token.Modules;

public static class ValidateCardAnswer
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public static async Task<IResult> validateCardAnswer(
    [FromBody] dynamic body,
    ICardHandler cardHandler
    )
    {
        dynamic variables = new Dictionary<string, dynamic>();

        var transitionName = body.GetProperty("LastTransition").ToString();

        var ibUserSerialized = body.GetProperty("ibUserSerialized").ToString();
        IBUser ibUser = JsonSerializer.Deserialize<IBUser>(ibUserSerialized);

        var cardNo = body.GetProperty("cardNo").ToString();
        var cardCvv = body.GetProperty("cardCvv").ToString();
        var cardPin = body.GetProperty("cardPin").ToString();

        var message  = new Dictionary<string, string>();

        ServiceResponse cardResponse = await cardHandler.ValidateCard(ibUser.UserName, cardNo, cardCvv, cardPin);
        if (cardResponse.StatusCode == 200)
        {
            variables.Add("isValidated",true);
        }
        else
        {
            variables.Add("isValidated",false);
            message = ErrorMessages.WrongCardInfo;
        }
        variables.Add("validateCardErrorMessage",message);
        return Results.Ok(variables);
    }


}
