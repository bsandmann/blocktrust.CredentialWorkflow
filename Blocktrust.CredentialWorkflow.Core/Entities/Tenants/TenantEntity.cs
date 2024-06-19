namespace Blocktrust.CredentialWorkflow.Core.Entities.Tenants;

using System.ComponentModel.DataAnnotations;
using Blocktrust.CredentialWorkflow.Core.Entities.Identity;
using Microsoft.EntityFrameworkCore;

public record TenantEntity
{
    public Guid TenantEntityId { get; init; }

    [Unicode(true)] [MaxLength(100)] public required string Name { get; init; }

    public DateTime CreatedUtc { get; init; }

    /// <summary>
    /// A tenant can have many pools
    /// </summary>
    // public IList<PoolEntity> PoolEntities { get; init; }

    /// <summary>
    /// A tenant can have many application users
    /// </summary>
    public IList<ApplicationUser> ApplicationUsers { get; init; }

    // /// <summary>
    // /// Configured trust registries for this tenant
    // /// </summary>
    // public IList<TrustRegistryEntity> TrustRegistryEntities { get; init; }
}