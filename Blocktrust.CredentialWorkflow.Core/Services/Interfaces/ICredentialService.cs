using FluentResults;

namespace Blocktrust.CredentialWorkflow.Core.Services.Interfaces;

public interface ICredentialService
{
    Task<Result<string>> IssueCredential(string subjectDid, string issuerDid, string claimsJson);
}