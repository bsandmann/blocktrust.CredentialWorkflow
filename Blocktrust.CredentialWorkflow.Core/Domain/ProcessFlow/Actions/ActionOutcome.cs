namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;

using Blocktrust.CredentialWorkflow.Core.Domain.Enums;

public class ActionOutcome
{
    public ActionOutcome(Guid actionId)
    {
        OutcomeId = Guid.NewGuid();
        ActionId = actionId;
        StartedUtc = DateTime.UtcNow;
    }

    public Guid OutcomeId { get; }
    public Guid ActionId { get; }
    public EActionOutcome EActionOutcome { get; private set; }
    public DateTime? StartedUtc { get; }
    public DateTime? EndedUtc { get; private set; }
    public string? ErrorJson { get; private set; }
    public string? OutcomeJson { get; private set; }

    public void FinishOutcomeWithFailure(string errorJson)
    {
        EndedUtc = DateTime.UtcNow;
        EActionOutcome = EActionOutcome.Failure;
        ErrorJson = errorJson;
    }

    public void FinishOutcomeWithSuccess(string outcomeJson)
    {
        EndedUtc = DateTime.UtcNow;
        EActionOutcome = EActionOutcome.Success;
        OutcomeJson = outcomeJson;
    }
}