using Blocktrust.CredentialWorkflow.Core.Domain.Credential;
using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.IssueCredentials.IssueW3cCredential;

public class IssueW3CCredentialRequest : IRequest<Result<string>>
{
    public IssueW3CCredentialRequest(Credential credential, byte[] privateKey, string issuerDid)
    {
        Credential = credential;
        PrivateKey = privateKey;
        IssuerDid = issuerDid;
    }

    public Credential Credential { get; }
    public byte[] PrivateKey { get; }
    public string IssuerDid { get; }
}