using Blocktrust.CredentialWorkflow.Core.Domain.Credential;
using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.VerifyCredentials.VerifyW3cCredentials.CheckRevocation;

public class CheckRevocationRequest : IRequest<Result<bool>>
{
    public CheckRevocationRequest(Credential credential)
    {
        Credential = credential;
    }

    public Credential Credential { get; }
}