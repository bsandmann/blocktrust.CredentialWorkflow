namespace Blocktrust.CredentialWorkflow.Core.Commands.IssueCredentials.IssueW3cCredential.SignW3cCredential;

using Blocktrust.CredentialWorkflow.Core.Domain.Credential;
using FluentResults;
using MediatR;

public class SignW3cCredentialRequest : IRequest<Result<string>>
{
    public SignW3cCredentialRequest(Credential credential, byte[] privateKey, string issuerDid)
    {
        Credential = credential;
        PrivateKey = privateKey;
        IssuerDid = issuerDid;
    }

    public Credential Credential { get; }
    public byte[] PrivateKey { get; }
    public string IssuerDid { get; }
}