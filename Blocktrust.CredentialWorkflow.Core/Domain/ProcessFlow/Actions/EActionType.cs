public enum EActionType
{
    // Credential Issuance Actions

    IssueW3CCredential,
    IssueW3CSdCredential,
    IssueAnoncredCredential,
    
    // Credential Verification Actions
    VerifyW3CCredential,
    VerifyW3CSdCredential,
    VerifyAnoncredCredential,

    // Communication Actions
    DidCommTrustPing,
    DidCommMessage,
    HttpPost,
    SendEmail,

   

}