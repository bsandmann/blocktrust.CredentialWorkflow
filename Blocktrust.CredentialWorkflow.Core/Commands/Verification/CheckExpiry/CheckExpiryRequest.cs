using Blocktrust.CredentialWorkflow.Core.Domain.Credential;
using MediatR;
using FluentResults;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Verification.CheckExpiry;

public class CheckExpiryRequest : IRequest<Result<bool>>
{
    public CheckExpiryRequest(Credential credential)
    {
        Credential = credential;
    }

    public Credential Credential { get; }
}