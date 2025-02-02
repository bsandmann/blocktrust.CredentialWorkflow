using Blocktrust.CredentialWorkflow.Core.Domain.IssuingKey;
using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Tenant.GetIssuingKeys;

public class GetIssuingKeysRequest : IRequest<Result<List<IssuingKey>>>
{
    public GetIssuingKeysRequest(Guid tenantId)
    {
        TenantId = tenantId;
    }

    public Guid TenantId { get; }
}