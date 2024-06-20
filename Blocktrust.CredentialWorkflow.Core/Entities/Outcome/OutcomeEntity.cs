namespace Blocktrust.CredentialWorkflow.Core.Entities.Outcome;

using System.Runtime.InteropServices.JavaScript;
using Domain.Enums;
using Domain.Outcome;
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

    public Outcome Map()
    {
        return new Outcome()
        {
            OutcomeId = this.OutcomeEntityId,
            OutcomeState = this.OutcomeState,
            StartedUtc = this.StartedUtc,
            EndedUtc = this.EndedUtc,
            ErrorJson = this.ErrorJson,
            OutcomeJson = this.OutcomeJson,
            WorkflowId = this.WorkflowEntityId
        };
    }
}