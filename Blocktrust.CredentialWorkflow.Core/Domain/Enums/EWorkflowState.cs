namespace Blocktrust.CredentialWorkflow.Core.Domain.Enums;

public enum EWorkflowState
{
    Inactive,
    ActiveWithExternalTrigger,
    ActiveWithRecurrentTrigger,
    ActiveWithFormTrigger

}