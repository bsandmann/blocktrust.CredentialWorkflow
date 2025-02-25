using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.GetPeerDIDs;

using Domain.PeerDID;

public class GetPeerDIDsRequest : IRequest<Result<List<PeerDIDModel>>>
{
    public GetPeerDIDsRequest(Guid tenantId)
    {
        TenantId = tenantId;
    }

    public Guid TenantId { get; }
}