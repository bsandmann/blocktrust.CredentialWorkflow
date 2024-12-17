namespace Blocktrust.CredentialWorkflow.Core.Entities.Tenant;

using System.ComponentModel.DataAnnotations;
using Domain.IssuingKey;
using Microsoft.EntityFrameworkCore;

public class IssuingKeyEntity
{
    public Guid IssuingKeyId { get; init; }

    [Unicode(true)] [MaxLength(100)] public required string Name { get; set; }

    public DateTime CreatedUtc { get; init; }

    [Unicode(true)] [MaxLength(1000)] public string KeyType { get; set; }
    [Unicode(true)] [MaxLength(1000)] public string PublicKey { get; set; }
    [Unicode(true)] [MaxLength(1000)] public string PrivateKey { get; set; }

    public Guid TenantEntityId { get; set; }

    public IssuingKey Map(IssuingKeyEntity issuingKeyEntity)
    {
        return new IssuingKey
        {
            IssuingKeyId = issuingKeyEntity.IssuingKeyId,
            Name = issuingKeyEntity.Name,
            CreatedUtc = issuingKeyEntity.CreatedUtc,
            KeyType = issuingKeyEntity.KeyType,
            PublicKey = issuingKeyEntity.PublicKey,
            PrivateKey = issuingKeyEntity.PrivateKey
        };
    }
}