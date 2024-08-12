
using System.Dynamic;
using System.Text.Json;
using amorphie.token.core.Models.InternetBanking;
using amorphie.token.data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace amorphie.token.Modules;

public static class GenerateResetPasswordQuestion
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public static async Task<IResult> generateResetPasswordQuestion(
    [FromBody] dynamic body,
    IbDatabaseContext ibContext
    )
    {
        dynamic variables = new Dictionary<string, dynamic>();

        var ibUserSerialized = body.GetProperty("ibUserSerialized").ToString();
        IBUser ibUser = JsonSerializer.Deserialize<IBUser>(ibUserSerialized);

        var securityQuestion = await ibContext.Question.Where(q => q.UserId == ibUser.Id)
                .OrderByDescending(q => q.CreatedAt).FirstOrDefaultAsync();

        PasswordHasher passwordHasher = new();
        // var answer = passwordHasher.DecryptString(securityQuestion.EncryptedAnswer, securityQuestion.Id.ToString("N")).Trim();
        var answer = securityQuestion.EncryptedAnswer;

        var securityQuestionDefined = await ibContext.QuestionDefinition.Where(q => q.IsActive && q.Id == securityQuestion.DefinitionId).Select(
                    q => new
                    {
                        DescriptionTr = q.DescriptionTr,
                        DescriptionEn = q.DescriptionEn,
                    }
                ).ToListAsync();
        var transitionName = body.GetProperty("LastTransition").ToString();
        var dataBody = body.GetProperty($"TRX-{transitionName}").GetProperty("Data");

        dynamic dataChanged = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(dataBody.ToString());

        dynamic targetObject = new System.Dynamic.ExpandoObject();

        targetObject.Data = dataChanged;

        var deviceId = body.GetProperty("Headers").GetProperty("xdeviceid").ToString();
        var installationId = body.GetProperty("Headers").GetProperty("xtokenid").ToString();
        dataChanged.additionalData = new ExpandoObject();

        if (answer.Length == 2)
        {
            dataChanged.additionalData.answerFirstCharIndex = 1;
            dataChanged.additionalData.answerSecondCharIndex = 2;
        }
        else
        {
            var rnd = new Random();
            int seperator = answer.Length / 2;
            dataChanged.additionalData.answerFirstCharIndex = rnd.Next(1, seperator);
            dataChanged.additionalData.answerSecondCharIndex = rnd.Next(seperator, answer.Length + 1);
        }
        dataChanged.additionalData.question = securityQuestionDefined;
        targetObject.Data = dataChanged;
        targetObject.TriggeredBy = Guid.Parse(body.GetProperty($"TRX-{transitionName}").GetProperty("TriggeredBy").ToString());
        targetObject.TriggeredByBehalfOf = Guid.Parse(body.GetProperty($"TRX-{transitionName}").GetProperty("TriggeredByBehalfOf").ToString());
        variables.Add($"TRX{transitionName.ToString().Replace("-", "")}", targetObject);
        variables.Add("answerFirstCharIndex", dataChanged.additionalData.answerFirstCharIndex);
        variables.Add("answerSecondCharIndex", dataChanged.additionalData.answerSecondCharIndex);
        return Results.Ok(variables);
    }


}
