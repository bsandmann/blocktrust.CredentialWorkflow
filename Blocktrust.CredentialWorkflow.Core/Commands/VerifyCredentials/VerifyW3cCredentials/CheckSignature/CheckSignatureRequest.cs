using Blocktrust.CredentialWorkflow.Core.Domain.Credential;
using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.VerifyCredentials.VerifyW3cCredentials.CheckSignature;

public class CheckSignatureRequest : IRequest<Result<bool>>
{
    public CheckSignatureRequest(Credential credential)
    {
        Credential = credential;
    }

    public Credential Credential { get; }
}
