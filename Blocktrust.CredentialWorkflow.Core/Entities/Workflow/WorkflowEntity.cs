namespace Blocktrust.CredentialWorkflow.Core.Entities.Workflow;

using System.ComponentModel.DataAnnotations;
using Domain.Enums;
using Domain.ProcessFlow;
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

    [Unicode(false)] public string? ProcessFlowJson { get; set; }

    public bool IsRunable { get; set; }

    // FK
    public TenantEntity TenantEntity { get; init; }
    public Guid TenantEntityId { get; init; }

    public List<WorkflowOutcomeEntity> WorkflowOutcomeEntities { get; init; }


    public Workflow Map()
    {
        var workflow = new Workflow()
        {
            Name = this.Name,
            WorkflowState = this.WorkflowState,
            CreatedUtc = this.CreatedUtc,
            UpdatedUtc = this.UpdatedUtc,
            ProcessFlowJson = this.ProcessFlowJson,
            WorkflowId = this.WorkflowEntityId,
            TenantId = this.TenantEntityId,
        };

        if (!string.IsNullOrEmpty(this.ProcessFlowJson))
        {
            workflow.ProcessFlow = ProcessFlow.DeserializeFromJson(ProcessFlowJson);
        }

        return workflow;
    }
}