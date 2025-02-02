using Blocktrust.CredentialWorkflow.Core.Domain.IssuingKey;
using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Tenant.CreateIssuingKey;

public class CreateIssuingKeyRequest : IRequest<Result<IssuingKey>>
{
    public CreateIssuingKeyRequest(Guid tenantId, string name, string did, string keyType, string publicKey, string privateKey)
    {
        TenantId = tenantId;
        Name = name;
        Did = did;
        KeyType = keyType;
        PublicKey = publicKey;
        PrivateKey = privateKey;
    }

    public Guid TenantId { get; }
    public string Name { get; }
    public string Did { get; }
    public string KeyType { get; }
    public string PublicKey { get; }
    public string PrivateKey { get; }
}