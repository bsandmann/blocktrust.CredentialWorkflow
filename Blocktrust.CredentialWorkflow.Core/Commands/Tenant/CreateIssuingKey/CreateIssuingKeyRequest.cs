using Blocktrust.CredentialWorkflow.Core.Domain.IssuingKey;
using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Tenant.CreateIssuingKey;

public class CreateIssuingKeyRequest : IRequest<Result<IssuingKey>>
{
    public CreateIssuingKeyRequest(Guid tenantId, string name, string did, string keyType, string privateKey, string publicKey, string? publicKey2)
    {
        TenantId = tenantId;
        Name = name;
        Did = did;
        KeyType = keyType;
        PublicKey = publicKey;
        PublicKey2 = publicKey2;
        PrivateKey = privateKey;
    }

    public Guid TenantId { get; }
    public string Name { get; }
    public string Did { get; }
    public string KeyType { get; }
    public string PublicKey { get; set; }
    public string? PublicKey2 { get; }
    public string PrivateKey { get; }
}