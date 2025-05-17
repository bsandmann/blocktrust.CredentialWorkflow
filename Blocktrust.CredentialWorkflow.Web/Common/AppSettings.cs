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
    
    /// <summary>
    /// Base URL for Prism DID resolver
    /// </summary>
    public string? PrismBaseUrl { get; set; }
    
    /// <summary>
    /// Default ledger for Prism DID resolver
    /// </summary>
    public string? PrismDefaultLedger { get; set; }
    
    /// <summary>
    /// Fallback base URL for Prism DID resolver
    /// </summary>
    public string? PrismBaseUrlFallback { get; set; }
    
    /// <summary>
    /// Fallback default ledger for Prism DID resolver
    /// </summary>
    public string? PrismDefaultLedgerFallback { get; set; }
}