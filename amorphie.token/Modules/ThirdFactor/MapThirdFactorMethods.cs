

namespace amorphie.token.Modules.ThirdFactor
{
    public static class MapThirdFactorMethods
    {
        public static void MapThirdFactorWorkflowEndpoints(this WebApplication app)
        {

            app.MapPost("/amorphie-third-factor-set-success", SetSuccess.setSuccess)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);

            app.MapPost("/amorphie-third-factor-set-error", SetError.setError)
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK);
            
        }



    }
}