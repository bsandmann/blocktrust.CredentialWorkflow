using Blocktrust.CredentialWorkflow.Core.Domain.Common;

namespace Blocktrust.CredentialWorkflow.Core.Domain.Workflow;

using Enums;
using ProcessFlow;
using ProcessFlow.Actions;
using Tenant;

public record Workflow
{
    public Guid WorkflowId { get; init; }

    public required string Name { get; set; }

    public required DateTime CreatedUtc { get; init; }
    public required DateTime UpdatedUtc { get; set; }

    public required EWorkflowState WorkflowState { get; set; }
    
    public string? ProcessFlowJson { get; set; }
    
    public ProcessFlow? ProcessFlow { get; set; }
    
    public Tenant Tenant { get; init; }
    public Guid TenantId { get; init; }
    
    public List<ActionOutcome> Outcomes { get; init; }
}