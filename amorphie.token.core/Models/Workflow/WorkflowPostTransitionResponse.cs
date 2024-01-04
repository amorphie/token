namespace amorphie.token.core.Models.Workflow;

public class WorkflowPostTransitionResponse
{
    public Result Result { get; set; }
}

public class Result
{
    public string Status { get; set; }
    public string Message { get; set; }
    public string MessageDetail { get; set; }
}