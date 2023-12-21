using System.Dynamic;
using System.Text.Json;
using amorphie.token.core.Models.InternetBanking;
using amorphie.token.data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace amorphie.token.Modules.Login
{
    public static class CheckSecurityQuestionChange
    {
        public static async Task<IResult> checkSecurityQuestionChange(
        [FromBody] dynamic body,
        [FromServices] IbDatabaseContext ibContext
        )
        {
            await Task.CompletedTask;

            var transitionName = body.GetProperty("LastTransition").ToString();

            var dataBody = body.GetProperty($"TRX-{transitionName}").GetProperty("Data");

            dynamic dataChanged = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(dataBody.ToString());

            dynamic targetObject = new System.Dynamic.ExpandoObject();

            targetObject.Data = dataChanged;

            var ibUserSerialized = body.GetProperty("ibUserSerialized").ToString();
            IBUser ibUser = JsonSerializer.Deserialize<IBUser>(ibUserSerialized);

            var securityQuestion = await ibContext.Question.Where(q => q.UserId == ibUser.Id)
                .OrderByDescending(q => q.CreatedAt).FirstOrDefaultAsync();

            dynamic variables = new Dictionary<string, dynamic>();
            if(securityQuestion == null)
            {
                variables.Add("status",true);
                variables.Add("changeSecurityQuestion",true);
            }
            else
            {
                var securityQuestionDefinition = await ibContext.QuestionDefinition.
                    FirstOrDefaultAsync(d => d.Id == securityQuestion.DefinitionId && d.IsActive && d.Type == 10);
                if(securityQuestionDefinition == null || securityQuestion?.Status != 10)
                {
                    variables.Add("status",true);
                    variables.Add("changeSecurityQuestion",true);
                }
                else
                {
                    variables.Add("status",true);
                    variables.Add("changeSecurityQuestion",false);
                }
            }
            
            if(variables["changeSecurityQuestion"] == true)
            {
                var securityQuestions = await ibContext.QuestionDefinition.Where(q => q.IsActive && q.Type == 10).Select(
                    q => new{
                        Id = q.Id,
                        DescriptionTr = q.DescriptionTr,
                        DescriptionEn = q.DescriptionEn,
                        Key = q.Key,
                        ValueTypeClr = q.ValueTypeClr,
                        Priority = q.Priority
                    }
                ).ToListAsync();
                dataChanged.additionalData = new ExpandoObject();
                dataChanged.additionalData.securityQuestions = securityQuestions;
                targetObject.Data = dataChanged;
                targetObject.TriggeredBy = Guid.Parse(body.GetProperty($"TRX-{transitionName}").GetProperty("TriggeredBy").ToString());
                targetObject.TriggeredByBehalfOf = Guid.Parse(body.GetProperty($"TRX-{transitionName}").GetProperty("TriggeredByBehalfOf").ToString());
                variables.Add($"TRX{transitionName.ToString().Replace("-","")}", targetObject);
            }

            return Results.Ok(variables);
        }
    }
}