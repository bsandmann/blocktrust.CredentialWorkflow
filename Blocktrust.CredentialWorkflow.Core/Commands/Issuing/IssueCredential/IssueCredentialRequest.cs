using Blocktrust.CredentialWorkflow.Core.Domain.Credential;
using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Issuing.IssueCredential;

public class IssueCredentialRequest : IRequest<Result<string>>
{
    public IssueCredentialRequest(Credential credential, byte[] privateKey, string issuerDid)
    {
        Credential = credential;
        PrivateKey = privateKey;
        IssuerDid = issuerDid;
    }

    public Credential Credential { get; }
    public byte[] PrivateKey { get; }
    public string IssuerDid { get; }
}