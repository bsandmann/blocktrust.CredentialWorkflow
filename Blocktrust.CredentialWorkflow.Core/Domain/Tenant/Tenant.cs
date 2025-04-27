namespace Blocktrust.CredentialWorkflow.Core.Domain.Tenant;

public record Tenant
{
    public Guid TenantId { get; init; }

    public string Name { get; init; }

    public DateTime CreatedUtc { get; init; }
    
    public string? OpnRegistrarUrl { get; init; }
    
    public string? WalletId { get; init; }
    
    public string? JwtPrivateKey { get; init; }
    
    public string? JwtPublicKey { get; init; }
}