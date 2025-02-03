namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;

using Blocktrust.CredentialWorkflow.Core.Domain.Enums;

public class ActionOutcome
{
    public Guid OutcomeId { get; set; }
    public Guid ActionId {get; set; }
    public EActionOutcome EActionOutcome { get; set; }
    public DateTime? StartedUtc { get; set; }
    public DateTime? EndedUtc { get; set; }
    public string? ErrorJson { get; set; }
    public string? OutcomeJson { get; set; }
}