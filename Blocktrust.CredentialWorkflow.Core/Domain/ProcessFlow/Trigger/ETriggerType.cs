namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Trigger;

public enum ETriggerType
{
    
    //Incoming requests triggers
    CredentialIssuanceTrigger,
    CredentialVerificationTrigger,
    
    //other triggers
    RecurringTimer,
    OnDemand
}