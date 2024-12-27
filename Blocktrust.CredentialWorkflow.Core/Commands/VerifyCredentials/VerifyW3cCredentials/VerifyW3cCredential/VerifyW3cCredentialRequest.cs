using Blocktrust.CredentialWorkflow.Core.Domain.Verification;
using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.VerifyCredentials.VerifyW3cCredentials.VerifyW3cCredential;

public class VerifyW3CCredentialRequest : IRequest<Result<CredentialVerificationResult>>
{
    public VerifyW3CCredentialRequest(string credential)
    {
        Credential = credential;
    }

    public string Credential { get; }
}