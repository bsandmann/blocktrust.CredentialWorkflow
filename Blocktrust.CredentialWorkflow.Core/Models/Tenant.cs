namespace Blocktrust.CredentialWorkflow.Core.Models;

public record Tenant
{
    public Guid TenantId { get; init; }
    
    public string Name { get; init; }
}