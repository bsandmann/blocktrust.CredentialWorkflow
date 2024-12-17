using FluentResults;
using MediatR;

public class DeleteIssuingKeyRequest : IRequest<Result>
{
    public DeleteIssuingKeyRequest(Guid issuingKeyId)
    {
        IssuingKeyId = issuingKeyId;
    }

    public Guid IssuingKeyId { get; }
}