using Blocktrust.CredentialWorkflow.Core.Domain.IssuingKey;
using FluentResults;
using MediatR;

public class CreateIssuingKeyRequest : IRequest<Result<IssuingKey>>
{
    public CreateIssuingKeyRequest(Guid tenantId, string name, string keyType, string publicKey, string privateKey)
    {
        TenantId = tenantId;
        Name = name;
        KeyType = keyType;
        PublicKey = publicKey;
        PrivateKey = privateKey;
    }

    public Guid TenantId { get; }
    public string Name { get; }
    public string KeyType { get; }
    public string PublicKey { get; }
    public string PrivateKey { get; }
}