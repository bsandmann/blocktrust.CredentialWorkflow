namespace Blocktrust.CredentialWorkflow.Core.Domain.Verification;

public class CredentialVerificationResult
{
    public bool IsValid => SignatureValid && !IsExpired && !IsRevoked && InTrustRegistry;
    public bool SignatureValid { get; set; }
    public bool IsExpired { get; set; }
    public bool IsRevoked { get; set; } // Default not revoked
    public bool InTrustRegistry { get; set; } = true;  // Default in registry
    public string? ErrorMessage { get; set; }

    public static CredentialVerificationResult CreateInvalid(string error)
    {
        return new CredentialVerificationResult { ErrorMessage = error };
    }
}