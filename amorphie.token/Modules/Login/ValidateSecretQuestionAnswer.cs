
using System.Dynamic;
using System.Text.Json;
using amorphie.token.core.Models.InternetBanking;
using amorphie.token.data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace amorphie.token.Modules;

public static class ValidateSecretQuestionAnswer
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public static async Task<IResult> validateSecretQuestionAnswer(
    [FromBody] dynamic body,
    IbDatabaseContext ibContext
    )
    {
        dynamic variables = new ExpandoObject();

        var message  = new Dictionary<string, string>();

        var transitionName = body.GetProperty("LastTransition").ToString();

        var ibUserSerialized = body.GetProperty("ibUserSerialized").ToString();
        IBUser ibUser = JsonSerializer.Deserialize<IBUser>(ibUserSerialized);

        var securityQuestion = await ibContext.Question.Where(q => q.UserId == ibUser.Id)
                .OrderByDescending(q => q.CreatedAt).FirstOrDefaultAsync();

        PasswordHasher passwordHasher = new();
        var answer = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT").Equals("Prod") ? passwordHasher.DecryptString(securityQuestion.EncryptedAnswer, securityQuestion.Id.ToString("N")).Trim() : securityQuestion.EncryptedAnswer;

        int firstCharIndex = Convert.ToInt32(body.GetProperty("answerFirstCharIndex").ToString());
        int secondCharIndex = Convert.ToInt32(body.GetProperty("answerSecondCharIndex").ToString());
        string firstCharAnswer = body.GetProperty("TRX-" + transitionName).GetProperty("Data").GetProperty("entityData").GetProperty("firstCharAnswer").ToString();
        string secondCharAnswer = body.GetProperty("TRX-" + transitionName).GetProperty("Data").GetProperty("entityData").GetProperty("secondCharAnswer").ToString();
        string actualFirstCharAnswer = answer[firstCharIndex - 1].ToString();
        string actualSecondCharAnswer = answer[secondCharIndex - 1].ToString();

        if (actualFirstCharAnswer.Equals(firstCharAnswer, StringComparison.OrdinalIgnoreCase)
        && actualSecondCharAnswer.Equals(secondCharAnswer, StringComparison.OrdinalIgnoreCase))
        {
            variables.isValidated = true;
        }
        else
        {
            variables.isValidated = false;
            message = ErrorMessages.WrongQuestionAnswer;
        }
        variables.validateQuestionErrorMessage = message;

        return Results.Ok(variables);
    }


}
