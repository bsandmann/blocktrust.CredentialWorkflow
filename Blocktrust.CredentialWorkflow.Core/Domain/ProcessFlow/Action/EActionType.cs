public enum EActionType
{
    // Credential Issuance Actions
    CreateCredential,
    SignCredential,
    CreateW3cCredential,
    CreateW3cSdCredential,
    CreateAnoncredCredential,
    
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