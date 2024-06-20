namespace Blocktrust.CredentialWorkflow.Core.Commands.Tenant.GetTenantInformation;

using Domain.Tenant;

public record GetTenantInformationResponse
{
    public Tenant Tenant { get; init; }
}