namespace Blocktrust.CredentialWorkflow.Core.Domain.Workflow;

using Enums;
using ProcessFlow;
using Tenant;

public record Workflow
{
    public Guid WorkflowId { get; init; }

    public required string Name { get; set; }

    public required DateTime CreatedUtc { get; init; }
    public required DateTime UpdatedUtc { get; set; }

    /// <summary>
    /// The state of the overall workflow. Is it running or not
    /// </summary>
    public required EWorkflowState WorkflowState { get; set; }

    public string? ProcessFlowJson { get; set; }

    public ProcessFlow? ProcessFlow { get; set; }

    public Tenant Tenant { get; init; }
    public Guid TenantId { get; init; }

    public bool IsRunable { get; init; }

    /// <summary>
    /// Each workflow usually runs multiple times. Each of those runs is a Workflow-Outcome
    /// </summary>
    public List<WorkflowOutcome> WorkflowOutcomes { get; init; }
}