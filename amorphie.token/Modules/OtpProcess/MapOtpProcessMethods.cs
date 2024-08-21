

namespace amorphie.token.Modules.OtpProcess
{
    public static class MapOtpProcessMethods
    {
        public static void MapOtpProcessWorkflowEndpoints(this WebApplication app)
        {

            app.MapPost("/amorphie-otp-process-set-variables", SetVariables.setVariables)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-otp-process-flow", OtpFlow.otpFlow)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-otp-process-check-otp", CheckOtpFlow.checkOtpFlow)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-otp-process-set-success", SetSuccess.setSuccess)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-otp-process-set-error", SetError.setError)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);
            
        }



    }
}