using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace token.Services.Tag;

public interface ITagService
{
    public Task<Dictionary<string,dynamic>> GetTagInfo(string domain,string entity,string tagName,string queryString);
}
