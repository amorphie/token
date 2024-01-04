

namespace amorphie.token.Services;

public class ServiceBase
{
    protected readonly IConfiguration Configuration;
    protected readonly ILogger Logger;
    public ServiceBase(ILogger logger, IConfiguration configuration)
    {
        Configuration = configuration;
        Logger = logger;
    }
}
