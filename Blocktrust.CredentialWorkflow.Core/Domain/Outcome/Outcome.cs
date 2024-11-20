namespace Blocktrust.CredentialWorkflow.Core.Domain.Outcome;

using Enums;
using Workflow;

public class Outcome
{
    public Guid OutcomeId { get; set; }
    
    public EOutcomeState OutcomeState { get; set; }
    
    public DateTime? StartedUtc { get; set; }
    
    public DateTime? EndedUtc { get; set; }
    
    public string? ErrorJson { get; set; }
    
    public string? OutcomeJson { get; set; }
   
    // FK
    public Workflow Workflow { get; set; }
    public Guid WorkflowId { get; set; }
}