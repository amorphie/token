using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace amorphie.token.core.Models;

public class ServiceResponseErrorModel
{
    public int StatusCode { get; set; }
    public string Detail { get; set; }
}

public class ServiceResponse<T>
{
    public int StatusCode { get; set; }
    public string Detail { get; set; } = "";
    public T? Response { get; set; }

    public ServiceResponseErrorModel GetErrorDetail()
    {
        if (StatusCode >= 400 && StatusCode < 500)
            return new ServiceResponseErrorModel { StatusCode = StatusCode, Detail = Detail };

        return new ServiceResponseErrorModel { StatusCode = StatusCode, Detail = "Internal Server Error" };

    }
}

public class ServiceResponse
{
    public int StatusCode { get; set; }
    public string Detail { get; set; } = "";

    public ServiceResponseErrorModel GetErrorDetail()
    {
        if (StatusCode >= 400 && StatusCode < 500)
            return new ServiceResponseErrorModel { StatusCode = StatusCode, Detail = Detail };

        return new ServiceResponseErrorModel { StatusCode = StatusCode, Detail = "Internal Server Error" };
    }
}
