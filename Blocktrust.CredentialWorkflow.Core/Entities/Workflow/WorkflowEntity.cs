namespace Blocktrust.CredentialWorkflow.Core.Entities.Workflow;

using System.ComponentModel.DataAnnotations;
using System.Data;
using Blocktrust.CredentialWorkflow.Core.Entities.Identity;
using Domain.Enums;
using Domain.Workflow;
using Microsoft.EntityFrameworkCore;
using Outcome;
using Tenant;

public record WorkflowEntity
{
    public Guid WorkflowEntityId { get; init; }

    [Unicode(false)] [MaxLength(100)] public required string Name { get; set; }

    public required DateTime CreatedUtc { get; init; }
    public required DateTime UpdatedUtc { get; set; }

    public required EWorkflowState WorkflowState { get; set; }

    [Unicode(false)] public string? ConfigurationJson { get; set; }


    // FK
    public TenantEntity TenantEntity { get; init; }
    public Guid TenantEntityId { get; init; }

    public List<OutcomeEntity> OutcomeEntities { get; init; }


    public Workflow Map()
    {
        return new Workflow()
        {
            Name = this.Name,
            WorkflowState = this.WorkflowState,
            CreatedUtc = this.CreatedUtc,
            UpdatedUtc = this.UpdatedUtc,
            ConfigurationJson = this.ConfigurationJson,
            WorkflowId = this.WorkflowEntityId,
            TenantId = this.TenantEntityId,
            Tenant = this.TenantEntity.Map(),
            Outcomes = this.OutcomeEntities.Select(p => p.Map()).ToList()
        };
    }
}