namespace Blocktrust.CredentialWorkflow.Core.Entities.Workflow;

using System.ComponentModel.DataAnnotations;
using Blocktrust.CredentialWorkflow.Core.Entities.Identity;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Outcome;
using Tenant;

public record WorkflowEntity
{
    public Guid WorkflowEntityId { get; init; }

    [Unicode(false)] [MaxLength(100)] public required string Name { get; init; }

    public DateTime CreatedUtc { get; init; }

    public EWorkflowState WorkflowState { get; init; }
    
    [Unicode(false)] 
    public string? ConfigurationJson { get; init; }
    
    
    // FK
    public TenantEntity TenantEntity { get; init; }
    public Guid TenantEntityId { get; init; }
    
    public List<OutcomeEntity> OutcomeEntities { get; init; }
}