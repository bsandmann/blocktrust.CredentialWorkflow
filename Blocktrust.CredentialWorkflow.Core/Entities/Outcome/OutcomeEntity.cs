using Blocktrust.CredentialWorkflow.Core.Domain.Common;
using Blocktrust.CredentialWorkflow.Core.Domain.Enums;
using Blocktrust.CredentialWorkflow.Core.Entities.Workflow;

namespace Blocktrust.CredentialWorkflow.Core.Entities.Outcome;

using Domain.ProcessFlow.Actions;

public class OutcomeEntity
{
    public Guid OutcomeEntityId { get; set; }
    public EOutcomeState OutcomeState { get; set; }
    public DateTime? StartedUtc { get; set; }
    public DateTime? EndedUtc { get; set; }
    public string? ErrorJson { get; set; }
    public string? OutcomeJson { get; set; }

    /// <summary>
    /// The context e.g. the paramters passed into the trigger as HTTP POST/GET
    /// </summary>
    public string? ExecutionContext { get; set; }

    // FK
    public WorkflowEntity? WorkflowEntity { get; set; }
    public Guid WorkflowEntityId { get; set; }

    public ActionOutcome Map()
    {
        return new ActionOutcome
        {
            OutcomeId = OutcomeEntityId,
            OutcomeState = OutcomeState,
            StartedUtc = StartedUtc,
            EndedUtc = EndedUtc,
            ErrorJson = ErrorJson,
            OutcomeJson = OutcomeJson,
            WorkflowId = WorkflowEntityId,
            Workflow = WorkflowEntity?.Map(),
            ExecutionContext = ExecutionContext
        };
    }
}