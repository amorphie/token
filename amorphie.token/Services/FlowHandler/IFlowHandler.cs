using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using amorphie.token.core.Models.Workflow;

namespace amorphie.token.Services.FlowHandler
{
    public interface IFlowHandler
    {
        public FlowProcess FlowProcess { get; }
        public Task<ServiceResponse> Init(string id);
        public Task<ServiceResponse> Save(FlowProcess flowProcess);
        public Task<FlowProcess> Wait(CancellationToken cancellationToken);
    }
}