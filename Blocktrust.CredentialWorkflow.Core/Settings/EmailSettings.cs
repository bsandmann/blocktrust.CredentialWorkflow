namespace Blocktrust.CredentialWorkflow.Core.Settings;

public class EmailSettings
{
    public string SendGridKey { get; set; } = string.Empty;
    public string SendGridFromEmail { get; set; } = string.Empty;
    public string DefaultFromName { get; set; } = string.Empty;
}