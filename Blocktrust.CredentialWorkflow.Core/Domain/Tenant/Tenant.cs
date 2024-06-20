namespace Blocktrust.CredentialWorkflow.Core.Domain.Tenant;

using Workflow;

public record Tenant
{
    public Guid TenantId { get; init; }

    public string Name { get; init; }

    public DateTime CreatedUtc { get; init; }

    /// <summary>
    /// A tenant can have many workflows
    /// </summary>
    public List<Workflow> WorkflowEntities { get; init; }
}