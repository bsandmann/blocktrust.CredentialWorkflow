namespace Blocktrust.CredentialWorkflow.Core.Domain.Workflow;

public class WorkflowSummary
{
    public Guid WorkflowId { get; init; }

    public required string Name { get; set; }

    public required DateTime UpdatedUtc { get; set; }
}