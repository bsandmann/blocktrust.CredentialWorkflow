using Blocktrust.CredentialWorkflow.Core.Domain.Verification;
using MediatR;

using FluentResults;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Verification.VerifyCredential;

public class VerifyCredentialRequest : IRequest<Result<CredentialVerificationResult>>
{
    public VerifyCredentialRequest(string credential)
    {
        Credential = credential;
    }

    public string Credential { get; }
}