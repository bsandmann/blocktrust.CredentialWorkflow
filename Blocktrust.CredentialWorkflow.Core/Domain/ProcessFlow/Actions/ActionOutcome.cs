namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;

using Blocktrust.CredentialWorkflow.Core.Domain.Enums;

public class ActionOutcome
{
    public Guid OutcomeId { get; set; }
    public EWorkflowOutcomeState WorkflowOutcomeState { get; set; }
    public DateTime? StartedUtc { get; set; }
    public DateTime? EndedUtc { get; set; }
    public string? ErrorJson { get; set; }
    public string? OutcomeJson { get; set; }

    public string? ExecutionContext { get; set; }
    public Workflow.Workflow Workflow { get; set; }
    public Guid WorkflowId { get; set; }

}