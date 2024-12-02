namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Triggers;

public enum ETriggerType
{
    CredentialIssuanceTrigger,
    CredentialVerificationTrigger,
    CustomIncomingTrigger,
    RecurringTimer,
    PresetTimer,
    WalletInteraction,
    ManualTrigger
}