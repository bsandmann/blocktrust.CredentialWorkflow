namespace Blocktrust.CredentialWorkflow.Core.Domain.Workflow;

using Enums;
using Outcome;

public class WorkflowWithLastResult
{
    public Guid WorkflowId { get; init; }

    public required string Name { get; set; }

    public required DateTime UpdatedUtc { get; set; }
    
    public EWorkflowState WorkflowState { get; set; }
    
    public Outcome? LastOutcome { get; set; }
}