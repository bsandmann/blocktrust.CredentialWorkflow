﻿namespace Blocktrust.CredentialWorkflow.Core.Entities.Workflow;

using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Text.Json;
using Blocktrust.CredentialWorkflow.Core.Entities.Identity;
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

    // FK
    public TenantEntity TenantEntity { get; init; }
    public Guid TenantEntityId { get; init; }

    public List<OutcomeEntity> OutcomeEntities { get; init; }


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