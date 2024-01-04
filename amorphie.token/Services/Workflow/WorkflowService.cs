


using amorphie.token.core.Models.Workflow;

namespace amorphie.token.Services.Workflow;

public class WorkflowService : ServiceBase, IWorkflowService
{
    private readonly DaprClient _daprClient;
    public WorkflowService(DaprClient daprClient, ILogger<UserService> logger, IConfiguration configuration) : base(logger, configuration)
    {
        _daprClient = daprClient;
    }

    public async Task<ServiceResponse<WorkflowPostTransitionResponse>> PostTransaction(WorkflowPostTransitionRequest workflowPostTransactionRequest)
    {
        throw new NotImplementedException();
    }
}
