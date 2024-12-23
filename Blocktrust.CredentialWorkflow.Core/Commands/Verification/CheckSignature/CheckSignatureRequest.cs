using Blocktrust.CredentialWorkflow.Core.Domain.Credential;
using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Verification.CheckSignature;

public class CheckSignatureRequest : IRequest<Result<bool>>
{
    public CheckSignatureRequest(Credential credential)
    {
        Credential = credential;
    }

    public Credential Credential { get; }
}
