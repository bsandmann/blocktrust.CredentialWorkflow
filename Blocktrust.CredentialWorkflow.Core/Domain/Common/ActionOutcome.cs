using Blocktrust.CredentialWorkflow.Core.Domain.Enums;

namespace Blocktrust.CredentialWorkflow.Core.Domain.Common;

public class ActionOutcome
{
    public Guid OutcomeId { get; set; }
    
    public EOutcomeState OutcomeState { get; set; }
    
    public DateTime? StartedUtc { get; set; }
    
    public DateTime? EndedUtc { get; set; }
    
    public string? ErrorJson { get; set; }
    
    public string? OutputJson { get; set; }
   
    // FK
    public Workflow.Workflow Workflow { get; set; }
    public Guid WorkflowId { get; set; }
}