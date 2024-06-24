namespace Blocktrust.CredentialWorkflow.Core.Entities.Tenant;

using System.ComponentModel.DataAnnotations;
using Blocktrust.CredentialWorkflow.Core.Entities.Identity;
using Domain.Tenant;
using Microsoft.EntityFrameworkCore;
using Workflow;

public record TenantEntity
{
    public Guid TenantEntityId { get; init; }

    [Unicode(true)] [MaxLength(100)] public required string Name { get; init; }

    public DateTime CreatedUtc { get; init; }

    /// <summary>
    /// A tenant can have many workflows
    /// </summary>
    public IList<WorkflowEntity> WorkflowEntities { get; init; }

    /// <summary>
    /// A tenant can have many application users
    /// </summary>
    public IList<ApplicationUser> ApplicationUsers { get; init; }
    
    /// <summary>
    /// A tenant can have many identus agents
    /// </summary>
    public IList<IdentusAgent> IdentusAgents { get; set; }
    
    
    public Tenant Map()
    {
        return new Tenant()
        {
            Name = this.Name,
            CreatedUtc = this.CreatedUtc,
            TenantId = this.TenantEntityId,
        };
    }
}