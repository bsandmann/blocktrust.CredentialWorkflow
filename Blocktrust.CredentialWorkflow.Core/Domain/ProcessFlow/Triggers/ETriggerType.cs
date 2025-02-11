namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Triggers;

public enum ETriggerType
{
    HttpRequest,
    RecurringTimer,
    WalletInteraction,
    ManualTrigger,
    Form
}