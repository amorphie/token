using amorphie.token.core.Enums.MessagingGateway;
using amorphie.token.core.Models.MessagingGateway;
using amorphie.token.core.Models.Workflow;
using amorphie.token.Services.MessagingGateway;
using amorphie.token.Services.TransactionHandler;
using Refit;
using ZstdSharp.Unsafe;

namespace amorphie.token.Services.FlowHandler
{
    public class FlowHandler : ServiceBase, IFlowHandler
    {
        private FlowProcess _flowProcess;
        private long _instanceKey;
        private long _jobKey;
        private DaprClient _daprClient;
        public FlowHandler(ILogger<FlowHandler> logger, IConfiguration configuration, DaprClient daprClient) : base(logger, configuration)
        {
            _daprClient = daprClient;
        }

        public FlowProcess FlowProcess => _flowProcess;

        public async Task<ServiceResponse> Init(string id)
        {
            _flowProcess = await _daprClient.GetStateAsync<FlowProcess>(Configuration!["DAPR_STATE_STORE_NAME"],"Flow_"+id);

            if(_flowProcess is not {})
            {
                _flowProcess = new FlowProcess()
                {
                    Id = id,
                    FlowStatus = FlowStatus.Active
                };
                await _daprClient.SaveStateAsync(Configuration["DAPR_STATE_STORE_NAME"],"Flow_"+id,_flowProcess,metadata: new Dictionary<string, string> { { "ttlInSeconds", "60" } });
            }

            return new ServiceResponse{
                StatusCode = 200
            };
        }

        public async Task<ServiceResponse> Save(FlowProcess flowProcess)
        {
            await _daprClient.SaveStateAsync(Configuration["DAPR_STATE_STORE_NAME"],"Flow_"+flowProcess.Id,flowProcess,metadata: new Dictionary<string, string> { { "ttlInSeconds", "60" } });
            return new ServiceResponse{
                StatusCode = 200
            };
        }

        public async Task<FlowProcess> Wait(CancellationToken cancellationToken)
        {
            while(!cancellationToken.IsCancellationRequested)
            {
                _flowProcess = await _daprClient.GetStateAsync<FlowProcess>(Configuration!["DAPR_STATE_STORE_NAME"],"Flow_"+_flowProcess!.Id);
                await Task.Delay(50);
                if(_flowProcess is {})
                { 
                    if(_flowProcess.FlowStatus != FlowStatus.Active)
                        break;
                }
            }

            if(cancellationToken.IsCancellationRequested)
            {
                _flowProcess = new FlowProcess();
                _flowProcess!.StatusCode = 500;
                _flowProcess.ErrorMessage = "Internal Server Error | Timeout";
            }

            return _flowProcess!;
        }
    }
}