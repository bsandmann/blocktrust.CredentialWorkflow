using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Tenant.DeleteIssuingKey;

public class DeleteIssuingKeyRequest : IRequest<Result>
{
    public DeleteIssuingKeyRequest(Guid issuingKeyId)
    {
        IssuingKeyId = issuingKeyId;
    }

    public Guid IssuingKeyId { get; }
}