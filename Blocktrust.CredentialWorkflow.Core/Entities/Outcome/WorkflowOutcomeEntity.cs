using Blocktrust.CredentialWorkflow.Core.Domain.Enums;
using Blocktrust.CredentialWorkflow.Core.Entities.Workflow;

namespace Blocktrust.CredentialWorkflow.Core.Entities.Outcome;

using Domain.Workflow;

public class WorkflowOutcomeEntity
{
    public Guid WorkflowOutcomeEntityId { get; set; }
    public EWorkflowOutcomeState WorkflowOutcomeState { get; set; }
    public DateTime? StartedUtc { get; set; }
    public DateTime? EndedUtc { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ActionOutcomesJson { get; set; }

    /// <summary>
    /// The context e.g. the paramters passed into the trigger as HTTP POST/GET
    /// </summary>
    public string? ExecutionContext { get; set; }

    // FK
    public WorkflowEntity? WorkflowEntity { get; set; }
    public Guid WorkflowEntityId { get; set; }

    public WorkflowOutcome Map()
    {
        return new WorkflowOutcome
        {
            WorkflowId = WorkflowEntityId,
            WorkflowOutcomeId = WorkflowOutcomeEntityId,
            WorkflowOutcomeState = WorkflowOutcomeState,
            StartedUtc = StartedUtc,
            EndedUtc = EndedUtc,
            ErrorJson = ErrorMessage,
            ActionOutcomesJson = ActionOutcomesJson,
            ExecutionContext = ExecutionContext
        };
    }
}