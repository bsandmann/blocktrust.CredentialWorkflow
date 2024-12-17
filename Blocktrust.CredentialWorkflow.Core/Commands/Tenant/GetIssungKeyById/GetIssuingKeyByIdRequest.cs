using Blocktrust.CredentialWorkflow.Core.Domain.IssuingKey;
using FluentResults;
using MediatR;

public class GetIssuingKeyByIdRequest : IRequest<Result<IssuingKey>>
{
    public GetIssuingKeyByIdRequest(Guid issuingKeyId)
    {
        IssuingKeyId = issuingKeyId;
    }

    public Guid IssuingKeyId { get; }
}