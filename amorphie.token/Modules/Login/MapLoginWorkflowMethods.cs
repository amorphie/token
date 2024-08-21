
using System.Dynamic;

namespace amorphie.token.Modules.Login
{
    public static class MapLoginWorkflowMethods
    {
        public static void MapLoginWorkflowEndpoints(this WebApplication app)
        {

            app.MapPost("/amorphie-login-check-mobile-client", CheckMobileClient.checkMobileClient)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-login-check-backoffice-client", CheckBackofficeClient.checkBackofficeClient)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-login-check-grant-type", CheckGrantType.checkGrantType)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-login-check-scopes", CheckScopes.checkScopes)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-login-check-user", CheckUser.checkUser)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-login-disable-user", DisableUser.disableUser)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-login-set-login-type", SetLoginType.setLoginType)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-login-otp-flow", LoginOtpFlow.loginOtpFlow)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-login-check-otp-flow", CheckOtpFlow.checkOtpFlow)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-login-clear-otp-flow", ClearOtpFlow.clearOtpFlow)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-login-check-password-change", CheckPasswordChange.checkPasswordChange)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-login-check-security-image-change", CheckSecurityImageChange.checkSecurityImageChange)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-login-check-security-question-change", CheckSecurityQuestionChange.checkSecurityQuestionChange)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-login-set-new-password", SetNewPassword.setNewPassword)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-login-set-new-security-image", SetNewSecurityImage.setNewSecurityImage)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-login-set-new-security-question", SetNewSecurityQuestion.setNewSecurityQuestion)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-login-check-documents", CheckDocuments.checkDocuments)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-login-check-consent", CheckConsent.checkConsent)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-login-save-consent", SaveConsent.saveConsent)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-login-generate-tokens", GenerateTokens.generateTokens)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-remember-password-validate-username-phone", CheckUserPhone.checkUserPhone)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-remember-password-get-question-info", GenerateResetPasswordQuestion.generateResetPasswordQuestion)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-remember-password-validate-question", ValidateSecretQuestionAnswer.validateSecretQuestionAnswer)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-remember-password-set-initial-screen", SetInitialScreen.checkInitialScreens)
           .ExcludeFromDescription()
           .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-mobile-remember-password-validate-identity", CheckConsent.checkConsent)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            //Ekyc processes 
            app.MapPost("/amorphie-ekyc-prepare", EkycPrepare.Prepare)
                .ExcludeFromDescription()
                .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-ekyc-ocr-check", EkycOcrCheck.Check)
               .ExcludeFromDescription()
               .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-ekyc-nfc-check", EkycNfcCheck.Check)
                .ExcludeFromDescription()
                .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-ekyc-face-check", EkcyFaceCheck.Check)
                .ExcludeFromDescription()
                .Produces(StatusCodes.Status200OK);

           

            app.MapPost("/amorphie-ekyc-connection-exit", EkycFailed.ConnectionExit)
                .ExcludeFromDescription()
                .Produces(StatusCodes.Status200OK);
            
            app.MapPost("/amorphie-ekyc-global-failed", EkycFailed.GlobalFailed)
                .ExcludeFromDescription()
                .Produces(StatusCodes.Status200OK);
                
            app.MapPost("/amorphie-ekyc-selfservice-check", EkycSelfServiceCheck.Check)
                .ExcludeFromDescription()
                .Produces(StatusCodes.Status200OK);
                
            app.MapPost("/amorphie-ekyc-status-check", EkycStatusCheck.Check)
                .ExcludeFromDescription()
                .Produces(StatusCodes.Status200OK);

             app.MapPost("/amorphie-ekyc-exit", EkycExit.Exit)
                .ExcludeFromDescription()
                .Produces(StatusCodes.Status200OK);

             app.MapPost("/amorphie-ekyc-end", EkycEnded.End)
                .ExcludeFromDescription()
                .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-ekyc-videocall-ready-check", EkycVideoCallReadyCheck.Check)
                .ExcludeFromDescription()
                .Produces(StatusCodes.Status200OK);
            
            app.MapPost("/amorphie-ekyc-set-additional-data", EkycSetAdditionalData.Add)
                .ExcludeFromDescription()
                .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-ekyc-verify-results", EkycStatusCheck.CheckForNonDeposit)
                .ExcludeFromDescription()
                .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-remember-password-sms", RememberPasswordSms.SendTempPassworSms)
                .ExcludeFromDescription()
                .Produces(StatusCodes.Status200OK);

            app.MapGet("/amorphie-login-test", () =>
            {
                dynamic dd = new ExpandoObject();
                var t = dd.GetProperty("test");
                return Results.Ok();
            })
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            #region IvrLogin Endpoints
            app.MapPost("/amorphie-ivr-login-customer-channels", IvrLoginEndpoints.GetCustomerChannelsAsync)
                .ExcludeFromDescription()
                .Produces(StatusCodes.Status200OK);
            
            app.MapPost("/amorphie-ivr-login-generate-otp", IvrLoginEndpoints.GenerateOtpAsync)
                .ExcludeFromDescription()
                .Produces(StatusCodes.Status200OK);
            
            app.MapPost("/amorphie-ivr-login-type", IvrLoginEndpoints.LoginTypeAsync)
                .ExcludeFromDescription()
                .Produces(StatusCodes.Status200OK);
            
            app.MapPost("/amorphie-ivr-login-check-sms-otp", IvrLoginEndpoints.CheckSmsOtpAsync)
                .ExcludeFromDescription()
                .Produces(StatusCodes.Status200OK);
            
            app.MapPost("/amorphie-ivr-login-authorize", IvrLoginEndpoints.AuthorizeAsync)
                .ExcludeFromDescription()
                .Produces(StatusCodes.Status200OK);
            #endregion

            app.MapPost("/amorphie-mobile-remember-password-switch-method", RememberPassword.SwitchMethod)
                .ExcludeFromDescription()
                .Produces(StatusCodes.Status200OK);
            app.MapPost("/amorphie-mobile-remember-password-validate-card", ValidateCardAnswer.validateCardAnswer)
                .ExcludeFromDescription()
                .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-mobile-remember-password-validate-question", ValidateSecretQuestionAnswer.validateSecretQuestionAnswer)
                .ExcludeFromDescription()
                .Produces(StatusCodes.Status200OK);

        }



    }
}
