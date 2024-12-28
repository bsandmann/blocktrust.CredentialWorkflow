using Blocktrust.CredentialWorkflow.Core.Domain.Credential;
using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.VerifyCredentials.VerifyW3cCredentials.CheckExpiry;

public class CheckExpiryRequest : IRequest<Result<bool>>
{
    public CheckExpiryRequest(Credential credential)
    {
        Credential = credential;
    }

    public Credential Credential { get; }
}