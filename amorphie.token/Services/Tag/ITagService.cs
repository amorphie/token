using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace amorphie.token.Services.Tag;

public interface ITagService
{
    public Task<ServiceResponse<Dictionary<string,dynamic>>> GetTagInfo(string domain,string entity,string tagName,string queryString);
}
