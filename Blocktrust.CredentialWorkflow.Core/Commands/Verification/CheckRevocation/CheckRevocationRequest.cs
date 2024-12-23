using Blocktrust.CredentialWorkflow.Core.Domain.Credential;
using MediatR;
using FluentResults;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Verification.CheckRevocation;

public class CheckRevocationRequest : IRequest<Result<bool>>
{
    public CheckRevocationRequest(Credential credential)
    {
        Credential = credential;
    }

    public Credential Credential { get; }
}