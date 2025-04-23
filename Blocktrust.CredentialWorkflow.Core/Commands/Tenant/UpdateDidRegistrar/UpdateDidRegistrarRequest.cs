namespace Blocktrust.CredentialWorkflow.Core.Commands.Tenant.UpdateDidRegistrar;

using FluentResults;
using MediatR;

public class UpdateDidRegistrarRequest : IRequest<Result>
{
    public UpdateDidRegistrarRequest(Guid tenantEntityId, string? opnRegistrarUrl, string? walletId)
    {
        TenantEntityId = tenantEntityId;
        OpnRegistrarUrl = opnRegistrarUrl;
        WalletId = walletId;
    }

    public Guid TenantEntityId { get; }
    public string? OpnRegistrarUrl { get; }
    public string? WalletId { get; }
}