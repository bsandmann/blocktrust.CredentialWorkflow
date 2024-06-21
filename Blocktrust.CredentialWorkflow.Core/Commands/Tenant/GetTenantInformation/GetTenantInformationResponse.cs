namespace Blocktrust.CredentialWorkflow.Core.Commands.Tenant.GetTenantInformation;

using Domain.Tenant;
using Domain.Workflow;

public record GetTenantInformationResponse
{
    public Tenant Tenant { get; init; }
    public List<WorkflowSummary> WorkflowSummaries { get; init; } = new List<WorkflowSummary>();
}