namespace Blocktrust.CredentialWorkflow.Core.Settings;

public class CredentialSettings
{
    public string DefaultIssuerDid { get; set; } = string.Empty;
    public string SigningKeyId { get; set; } = string.Empty;
    public Dictionary<string, string> DefaultClaimTemplates { get; set; } = new();
}