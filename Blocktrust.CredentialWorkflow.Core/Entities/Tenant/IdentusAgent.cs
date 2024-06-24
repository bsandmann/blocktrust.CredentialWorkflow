namespace Blocktrust.CredentialWorkflow.Core.Entities.Tenant;

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

public class IdentusAgent
{
    public Guid IdentusAgentId { get; set; }
    
    [MaxLength(100)]
    public string? Name { get; set; }
    
    [MaxLength(200)]
    [Unicode(false)]
    public required Uri Uri { get; set; }
    
    [MaxLength(200)]
    [Unicode(false)]
    public required string ApiKey { get; set; }
    
    // FK
    public Guid TenantId { get; set; }
    public TenantEntity Tenant { get; set; }
}