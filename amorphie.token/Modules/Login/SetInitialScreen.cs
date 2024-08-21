using System.Dynamic;
using System.Text.Json;
using amorphie.token.core;
using amorphie.token.core.Models.InternetBanking;
using amorphie.token.data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace amorphie.token.Modules.Login
{
    public static class SetInitialScreen
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        public static async Task<IResult> checkInitialScreens(
        [FromBody] dynamic body,
        [FromServices] IbDatabaseContext ibContext,
        [FromServices] IPasswordRememberService passwordRememberService,
        [FromServices] IEkycProvider ekycProvider,
        IConfiguration configuration

        )
        {
            // await Task.CompletedTask;

            var transitionName = body.GetProperty("LastTransition").ToString();

            var dataBody = body.GetProperty($"TRX-{transitionName}").GetProperty("Data");
            var requestBodySerialized = body.GetProperty("requestBody").ToString();
            TokenRequest request = JsonSerializer.Deserialize<TokenRequest>(requestBodySerialized);

            dynamic dataChanged = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(dataBody.ToString());

            dynamic targetObject = new System.Dynamic.ExpandoObject();

            targetObject.Data = dataChanged;

            var ibUserSerialized = body.GetProperty("ibUserSerialized").ToString();
            IBUser ibUser = JsonSerializer.Deserialize<IBUser>(ibUserSerialized);
            

            var securityQuestion = await ibContext.Question.Where(q => q.UserId == ibUser.Id)
                .OrderByDescending(q => q.CreatedAt).FirstOrDefaultAsync();

            var hasQuestion = false;
            var hasCard = false;
            var hasVideoCall = false;
            var hasNfc = dataChanged.entityData.HasNfc;
            var hasNewIdentityCard = true;
            Dictionary<string, string> switchMethodErrorMessage = new();


            dynamic variables = new Dictionary<string, dynamic>();

           
            if (securityQuestion == null)
            {
                // variables.Add("status", true);
                variables.Add("changeSecurityQuestion", true);
            }
            else
            {
                var securityQuestionDefinition = await ibContext.QuestionDefinition.
                    FirstOrDefaultAsync(d => d.Id == securityQuestion.DefinitionId && d.IsActive && d.Type == 10);
                if (securityQuestionDefinition == null || securityQuestion?.Status != 10)
                {
                    // variables.Add("status", true);

                }
                else
                {
                    // variables.Add("status", true);
                    hasQuestion = true;
                }
            }

            var hasCards = await passwordRememberService.HasCardAsync(request.Username);
            if (hasCards.Response)
            {
                hasCard = true;
            }
            var videoCallAvailable = await passwordRememberService.VideoCallAvailableAsync();
            // Has nfc ?

            if(videoCallAvailable.Response.VideoCallAvailable){
                hasVideoCall = true;
            }

            // Has new identity card
            var kpsResult = new KpsIdentity();
            try
            {
                long tckn = Convert.ToInt64(body.UserName);
                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                var type = env == "Development" ? 1 : 0;
                kpsResult = await ekycProvider.GetKpsIdentityInfoAsync(tckn, null, type);
            }
            catch (System.Exception)
            {
                hasNewIdentityCard = false;
            }

             if (kpsResult is not null && kpsResult.CertificationType == KpsCertificationType.OldCertificate){
                hasNewIdentityCard = false;
             }
            if(!hasNewIdentityCard){
                switchMethodErrorMessage = ErrorMessages.OldIdentityCard;
            }
            
            variables.Add("citizenshipNumber", ibUser.UserName);
            variables.Add("HasQuestion",hasQuestion);
            variables.Add("HasCard",hasCard);

            variables.Add("HasVideoCall",hasVideoCall);
            variables.Add("HasNfc",hasNfc);
            variables.Add("HasNewIdentityCard",hasNewIdentityCard);
            variables.Add("switchMethodErrorMessage",switchMethodErrorMessage);
            variables.Add("EkycIsActive",videoCallAvailable.Response.IsActive);
            variables.Add("BasePath",configuration["BasePath"]);
                        



            return Results.Ok(variables);
        }
    }
}