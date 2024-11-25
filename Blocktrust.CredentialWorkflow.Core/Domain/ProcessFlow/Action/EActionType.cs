public enum EActionType
{
    // Credential Issuance Actions
    CreateCredential,
    SignCredential,
    
    // Credential Verification Actions
    CheckSignature,
    CheckExpiry,
    CheckRevocation,
    CheckTrustRegistry,
    
    // Communication Actions
    DIDCommTrustPing,
    DIDCommMessage,
    HTTPPost,
    SendEmail
}