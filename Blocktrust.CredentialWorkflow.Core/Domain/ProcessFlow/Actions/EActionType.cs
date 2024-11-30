public enum EActionType
{
    // Credential Issuance Actions
    CreateCredential,
    SignCredential,
    IssueW3cCredential,
    IssueW3cSdCredential,
    IssueAnoncredCredential,
    
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