using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace amorphie.token.core.Exceptions;

public class ServiceException : Exception
{
    public int Code { get; set; }
    public ServiceException(int code, string message) : base(message)
    {
        Code = code;
    }
}
