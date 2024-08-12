using System.Dynamic;
using System.Text.Json;
using amorphie.token.core.Models.InternetBanking;
using amorphie.token.data;
using amorphie.token.Services.InternetBanking;
using amorphie.token.Services.Migration;
using amorphie.token.Services.Profile;
using amorphie.token.Services.TransactionHandler;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace amorphie.token.Modules.Login
{
    public static class CheckUser
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        public static async Task<IResult> checkUser(
        [FromBody] dynamic body,
        [FromServices] IInternetBankingUserService internetBankingUserService,
        [FromServices] IProfileService profileService,
        [FromServices] IUserService userService,
        [FromServices] IbDatabaseContext ibContext,
        [FromServices] IbDatabaseContextMordor ibContextMordor,
        [FromServices] ITransactionService transactionService,
        [FromServices] IMigrationService migrationService
        )
        {
            var langCode = ErrorHelper.GetLangCode(body);
            var transitionName = body.GetProperty("LastTransition").ToString();
            var requestBodySerialized = body.GetProperty("TRX-" + transitionName).GetProperty("Data").GetProperty(WorkflowConstants.ENTITY_DATA_FIELD).ToString();
            TokenRequest request = JsonSerializer.Deserialize<TokenRequest>(requestBodySerialized);

            dynamic variables = new ExpandoObject();
            variables.requestBody = requestBodySerialized;
            variables.PasswordTryCount = 0;
            variables.wrongCredentials = false;
            variables.disableUser = false;

            if(!request.ClientId.Equals("backoffice"))
            {
            var userResponse = await internetBankingUserService.GetUser(request.Username!);
            if (userResponse.StatusCode != 200)
            {
                variables.status = false;
                variables.message = ErrorHelper.GetErrorMessage(LoginErrors.UserNotFound, langCode);
                variables.wrongCredentials = true;
                return Results.Ok(variables);
            }
            var user = userResponse.Response;
            variables.ibUserSerialized = JsonSerializer.Serialize(user);

            var userStatus = await ibContext.Status.Where(s => s.UserId == user!.Id && (!s.State.HasValue || s.State.Value == 10)).OrderByDescending(s => s.CreatedAt).FirstOrDefaultAsync();
            if (userStatus?.Type == 30 || userStatus?.Type == 40)
            {
                variables.status = false;
                variables.message = ErrorHelper.GetErrorMessage(LoginErrors.BlockedUser, langCode);
                variables.wrongCredentials = false;
                return Results.Ok(variables);
            }

            var passwordResponse = await internetBankingUserService.GetPassword(user!.Id);
            if (passwordResponse.StatusCode != 200)
            {
                variables.status = false;
                variables.message = ErrorHelper.GetErrorMessage(LoginErrors.UserNotFound, langCode);
                variables.wrongCredentials = true;
                return Results.Ok(variables);
            }
            var passwordRecord = passwordResponse.Response;

            var isVerified = internetBankingUserService.VerifyPassword(passwordRecord!.HashedPassword!, request.Password!, passwordRecord.Id.ToString());
            //Consider SuccessRehashNeeded
            if (isVerified != PasswordVerificationResult.Success)
            {
                transactionService.Logon.FailedLogons.Add(new FailedLogon
                {
                    ClientId = request.ClientId,
                    Reference = request.Username
                });

                variables.status = false;
                variables.message = ErrorHelper.GetErrorMessage(LoginErrors.WrongPassword, langCode); 
                passwordRecord.AccessFailedCount = (passwordRecord.AccessFailedCount ?? 0) + 1;
                variables.PasswordTryCount = passwordRecord.AccessFailedCount;
                variables.wrongCredentials = true;
                if (passwordRecord.AccessFailedCount >= 5)
                {
                    variables.disableUser = true;
                }

                await ibContext.SaveChangesAsync();
                return Results.Ok(variables);
            }
            else
            {
                passwordRecord.AccessFailedCount = 0;
                await ibContext.SaveChangesAsync();
            }

            
            var role = await ibContext.Role.Where(r => r.UserId.Equals(user!.Id) && r.Channel.Equals(10) && r.Status.Equals(10)).OrderByDescending(r => r.CreatedAt).FirstOrDefaultAsync();
            if(role is {} && (role.ExpireDate ?? DateTime.MaxValue) > DateTime.Now)
            {
                var roleDefinition = await ibContext.RoleDefinition.FirstOrDefaultAsync(d => d.Id.Equals(role.DefinitionId) && d.IsActive);
                if(roleDefinition is {})
                {
                    if(roleDefinition.Key == 0)
                    {   
                        variables.status = false;
                        variables.message = ErrorHelper.GetErrorMessage(LoginErrors.NotAuthorized, langCode);
                        variables.wrongCredentials = true;
                        return Results.Ok(variables);
                    }
                    else
                    {
                        variables.UserRoleKey = roleDefinition.Key;
                    }
                }
            }
            
            var userInfoResult = await profileService.GetCustomerSimpleProfile(request.Username!);
            if (userInfoResult.StatusCode != 200)
            {
                variables.status = false;
                variables.message = ErrorHelper.GetErrorMessage(LoginErrors.UserNotFound, langCode);
                variables.wrongCredentials = true;
                return Results.Ok(variables);
            }
        
            var userInfo = userInfoResult.Response;

            if (userInfo!.data!.profile!.Equals("customer") || !userInfo!.data!.profile!.status!.Equals("active"))
            {
                variables.status = false;
                variables.message = ErrorHelper.GetErrorMessage(LoginErrors.General, langCode);
                variables.wrongCredentials = true;
                return Results.Ok(variables);
            }

            var mobilePhoneCount = userInfo!.data!.phones!.Count(p => p.type!.Equals("mobile"));
            if (mobilePhoneCount != 1)
            {
                variables.status = false;
                variables.message = ErrorHelper.GetErrorMessage(LoginErrors.General, langCode);

                return Results.Ok(variables);
            }

            var mobilePhone = userInfo!.data!.phones!.FirstOrDefault(p => p.type!.Equals("mobile"));
            if (string.IsNullOrWhiteSpace(mobilePhone!.prefix) || string.IsNullOrWhiteSpace(mobilePhone!.number))
            {
                variables.status = false;
                variables.message = ErrorHelper.GetErrorMessage(LoginErrors.General, langCode);
                return Results.Ok(variables);
            }

            var userRequest = new UserInfo
            {
                firstName = userInfo!.data.profile!.name!,
                lastName = userInfo!.data.profile!.surname!,
                phone = new core.Models.User.UserPhone()
                {
                    countryCode = mobilePhone!.countryCode!,
                    prefix = mobilePhone!.prefix,
                    number = mobilePhone!.number
                },
                state = "Active",
                salt = passwordRecord.Id.ToString(),
                password = request.Password!,
                explanation = "Migrated From IB",
                reason = "Amorphie Login",
                isArgonHash = true
            };

            var verifiedMailAddress = userInfo.data.emails!.FirstOrDefault(m => m.isVerified == true);
            userRequest.eMail = verifiedMailAddress?.address ?? "";
            userRequest.reference = request.Username!;

            var migrateResult = await userService.SaveUser(userRequest);
            var amorphieUserResult = await userService.Login(new LoginRequest() { Reference = request.Username!, Password = request.Password! });
            var amorphieUser = amorphieUserResult.Response;

            var migrateUserInfoResult = await migrationService.MigrateUserData(amorphieUser.Id,user.Id);

            variables.status = true;

            variables.userInfo = userInfo;
            variables.BusinessLine = userInfo.data.profile.businessLine.Equals("X") ? "X" : "B";
            variables.Reference = amorphieUser.Reference;
            variables.userInfoSerialized = JsonSerializer.Serialize(userInfo);
            variables.userSerialized = JsonSerializer.Serialize(amorphieUser);
            variables.passwordSerialized = JsonSerializer.Serialize(passwordRecord);

            await ibContext.SaveChangesAsync();
            return Results.Ok(variables);
            }
            else
            {
                var amorphieUserResult = await userService.Login(new LoginRequest() { Reference = request.Username!, Password = request.Password! });
                if(amorphieUserResult.StatusCode != 200)
                {
                    variables.status = false;
                    variables.message = ErrorHelper.GetErrorMessage(LoginErrors.WrongPassword, langCode); 
                    variables.PasswordTryCount = 0;
                    variables.wrongCredentials = true;
                }
                var amorphieUser = amorphieUserResult.Response;
                variables.userSerialized = JsonSerializer.Serialize(amorphieUser);
                variables.Reference = amorphieUser.Reference;
                variables.status = true;
                return Results.Ok(variables);
            }
        }
    }
    
}