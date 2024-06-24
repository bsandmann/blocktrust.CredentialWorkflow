namespace Blocktrust.CredentialWorkflow.Core.Commands.Tenant.RemoveIdentusAgent;

using FluentResults;
using MediatR;

public class RemoveIdentusAgentRequest : IRequest<Result>
{
    public RemoveIdentusAgentRequest(Guid tenantId, Guid identusAgentId)
    {
        TenantId = tenantId;
        IdentusAgentId = identusAgentId;
    }

    public Guid TenantId { get; }
    public Guid IdentusAgentId { get; }
}