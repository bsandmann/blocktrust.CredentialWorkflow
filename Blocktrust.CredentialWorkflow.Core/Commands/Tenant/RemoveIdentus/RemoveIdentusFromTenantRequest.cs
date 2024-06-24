namespace Blocktrust.CredentialWorkflow.Core.Commands.Tenant.RemoveIdentus;

using FluentResults;
using MediatR;

public class RemoveIdentusFromTenantRequest : IRequest<Result>
{
    public RemoveIdentusFromTenantRequest(Guid tenantId, Guid identusAgentId)
    {
        TenantId = tenantId;
        IdentusAgentId = identusAgentId;
    }

    public Guid TenantId { get; }
    public Guid IdentusAgentId { get; }
}