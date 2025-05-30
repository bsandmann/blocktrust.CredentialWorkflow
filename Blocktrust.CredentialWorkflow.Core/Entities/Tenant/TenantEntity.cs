﻿namespace Blocktrust.CredentialWorkflow.Core.Entities.Tenant;

using System.ComponentModel.DataAnnotations;
using DIDComm;
using Identity;
using Domain.Tenant;
using Microsoft.EntityFrameworkCore;
using Workflow;

public record TenantEntity
{
    public Guid TenantEntityId { get; init; }

    [Unicode(true)] [MaxLength(100)] public required string Name { get; init; }

    public DateTime CreatedUtc { get; init; }
    
    /// <summary>
    /// OPN Registrar URL for DID registration
    /// </summary>
    [Unicode(true)] [MaxLength(255)] public string? OpnRegistrarUrl { get; set; }
    
    /// <summary>
    /// Wallet ID for DID registration
    /// </summary>
    [Unicode(true)] [MaxLength(100)] public string? WalletId { get; set; }
    
    /// <summary>
    /// JWT private key for signing tokens (RSA private key in XML format)
    /// </summary>
    [Unicode(true)] [MaxLength(4000)] public string? JwtPrivateKey { get; set; }
    
    /// <summary>
    /// JWT public key for validating tokens (RSA public key in XML format)
    /// </summary>
    [Unicode(true)] [MaxLength(2000)] public string? JwtPublicKey { get; set; }

    /// <summary>
    /// A tenant can have many workflows
    /// </summary>
    public IList<WorkflowEntity> WorkflowEntities { get; init; }

    /// <summary>
    /// A tenant can have many application users
    /// </summary>
    public IList<ApplicationUser> ApplicationUsers { get; init; }

    /// <summary>
    /// A tenant can have many issuing keys
    /// </summary>
    public IList<IssuingKeyEntity> IssuingKeys { get; set; }

    public IList<PeerDIDEntity> PeerDIDEntities { get; init; }

}