namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Triggers;

public enum ETriggerType
{
    
    //Incoming requests triggers
    CredentialIssuanceTrigger,
    CredentialVerificationTrigger,
    
    //other triggers
    RecurringTimer,
    OnDemand
}