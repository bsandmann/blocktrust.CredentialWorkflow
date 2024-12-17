using Blocktrust.CredentialWorkflow.Core.Domain.IssuingKey;
using FluentResults;
using MediatR;

public class UpdateIssuingKeyRequest : IRequest<Result<IssuingKey>>
{
    public UpdateIssuingKeyRequest(Guid issuingKeyId, string? name = null, string? keyType = null, string? publicKey = null, string? privateKey = null)
    {
        IssuingKeyId = issuingKeyId;
        Name = name;
        KeyType = keyType;
        PublicKey = publicKey;
        PrivateKey = privateKey;
    }

    public Guid IssuingKeyId { get; }
    public string? Name { get; }
    public string? KeyType { get; }
    public string? PublicKey { get; }
    public string? PrivateKey { get; }
}