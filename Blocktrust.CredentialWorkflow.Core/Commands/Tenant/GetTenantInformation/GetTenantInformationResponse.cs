namespace Blocktrust.CredentialWorkflow.Core.Commands.Tenant.GetTenantInformation;

using Blocktrust.CredentialWorkflow.Core.Models;

public record GetTenantInformationResponse
{
    public Tenant Tenant { get; init; }
}