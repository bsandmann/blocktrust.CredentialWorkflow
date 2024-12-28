using Blocktrust.CredentialWorkflow.Core.Domain.Credential;
using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.IssueCredentials.IssueW3cCredential.CreateW3cCredential;

public class CreateW3cCredentialRequest : IRequest<Result<Credential>>
{
    public CreateW3cCredentialRequest(
        string issuerDid, 
        string subjectDid,
        Dictionary<string, object>? additionalSubjectData = null,
        DateTimeOffset? validFrom = null,
        DateTimeOffset? expirationDate = null)
    {
        IssuerDid = issuerDid;
        SubjectDid = subjectDid;
        AdditionalSubjectData = additionalSubjectData;
        ValidFrom = validFrom ?? DateTimeOffset.UtcNow;
        ExpirationDate = expirationDate;
    }

    public string IssuerDid { get; }
    public string SubjectDid { get; }
    public Dictionary<string, object>? AdditionalSubjectData { get; }
    public DateTimeOffset ValidFrom { get; }
    public DateTimeOffset? ExpirationDate { get; }
}