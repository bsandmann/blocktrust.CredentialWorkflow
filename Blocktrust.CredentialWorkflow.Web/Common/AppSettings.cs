namespace Blocktrust.CredentialWorkflow.Web.Common;

/// <summary>
/// AppSettings-Configuration for the app
/// </summary>
public class AppSettings
{
    /// <summary>
    /// API key for SendGrid
    /// </summary>
    public string? SendGridKey { get; set; }

    /// <summary>
    /// Configured Email for SendGrid
    /// </summary>
    public string? SendGridFromEmail { get; set; }
}