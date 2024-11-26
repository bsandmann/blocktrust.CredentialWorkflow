public enum EActionType
{
    // Credential Issuance Actions
    CreateCredential,
    SignCredential,
    CreateW3cCredential,
    CreateW3cSdCredential,
    CreateAnoncredCredential,
    
    // Credential Verification Actions
    VerifyW3cCredential,
    VerifyW3cSdCredential,
    VerifyAnoncredCredential,

    // Communication Actions
    DIDCommTrustPing,
    DIDCommMessage,
    HTTPPost,
    SendEmail,
    
    
   
   

}