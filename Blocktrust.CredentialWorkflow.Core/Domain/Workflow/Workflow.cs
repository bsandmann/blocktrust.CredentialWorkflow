﻿namespace Blocktrust.CredentialWorkflow.Core.Domain.Workflow;

using Enums;
using Outcome;
using Tenant;

public record Workflow
{
    public Guid WorkflowId { get; init; }

    public required string Name { get; set; }

    public required DateTime CreatedUtc { get; init; }
    public required DateTime UpdatedUtc { get; set; }

    public required EWorkflowState WorkflowState { get; set; }
    
    public string? ConfigurationJson { get; set; }
    
    public Tenant Tenant { get; init; }
    public Guid TenantId { get; init; }
    
    public List<Outcome> Outcomes { get; init; }
}