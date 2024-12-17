namespace Blocktrust.CredentialWorkflow.Core.Domain.Workflow;

public static class WorkflowNaming
{
    public static Dictionary<string, string> FriendlyNames = new()
    {
        // Triggers
        {"CredentialIssuanceTrigger", "Http Request for Credential Issuance"},
        {"CredentialVerificationTrigger", "Http Request for Credential Verification"},
        {"CustomIncomingTrigger", "Custom Incoming Http Request"},
        {"RecurringTimer", "Recurring Timer"},
        {"PresetTimer", "Preset Timer"},
        {"WalletInteraction", "Wallet Interaction"},
        {"ManualTrigger", "Manual Trigger"},
        
        // Actions
        {"IssueW3CCredential", "Issue W3C Credential"},
        {"IssueW3CSdCredential", "Issue W3C SD Credential"},
        {"IssueAnoncredCredential", "Issue Anoncred Credential"},
        {"VerifyW3CCredential", "Verify W3C Credential"},
        {"VerifyW3CSdCredential", "Verify W3C SD Credential"},
        {"VerifyAnoncredCredential", "Verify Anoncred Credential"},
        {"DidCommTrustPing", "DIDComm Trust Ping"},
        {"DidCommMessage", "DIDComm Message"},
        {"HttpPost", "HTTP Post"},
        {"SendEmail", "Send Email"}
    };

    public static string GetFriendlyName(string technicalName)
    {
        return FriendlyNames.TryGetValue(technicalName, out var friendlyName) 
            ? friendlyName 
            : technicalName;
    }
}