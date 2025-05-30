namespace Blocktrust.CredentialWorkflow.Core.Domain.Workflow;

using Enums;
using ProcessFlow;

public class WorkflowSummary
{
    public Guid WorkflowId { get; init; }

    public required string Name { get; set; }

    public required DateTime UpdatedUtc { get; set; }

    public required EWorkflowState WorkflowState { get;set; }

    public WorkflowOutcome? LastWorkflowOutcome { get;set; }

    public bool IsRunable { get; set; }
    
    public string? ProcessFlowJson { get; set; }
    
    public ProcessFlow? ProcessFlow { get; set; }
}