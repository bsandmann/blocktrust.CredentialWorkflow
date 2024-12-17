namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;

public enum EActionType
{
    IssueW3CCredential,
    IssueW3CSdCredential,
    IssueAnoncredCredential,
    VerifyW3CCredential,
    VerifyW3CSdCredential,
    VerifyAnoncredCredential,
    DIDComm,
    Http,
    Email,
    LogOutcome,
    PostOutcome
}