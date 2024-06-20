namespace Blocktrust.CredentialWorkflow.Core.Entities.Outcome;

using System.Runtime.InteropServices.JavaScript;
using Domain.Enums;
using Workflow;

public class OutcomeEntity
{
    public Guid OutcomeEntityId { get; set; }
    
    public EOutcomeState OutcomeState { get; set; }
    
    public DateTime? StartedUtc { get; set; }
    
    public DateTime? EndedUtc { get; set; }
    
    public string? ErrorJson { get; set; }
    
    public string? OutcomeJson { get; set; }
    
    // FK
    public WorkflowEntity WorkflowEntity { get; set; }
    public Guid WorkflowEntityId { get; set; }
}