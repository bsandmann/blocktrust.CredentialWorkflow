using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.GetPeerDidById
{
    using Domain.PeerDID;

    public class GetPeerDidByIdRequest : IRequest<Result<PeerDIDModel>>
    {
        public Guid PeerDidEntityId { get; }

        public GetPeerDidByIdRequest(Guid peerDidEntityId)
        {
            PeerDidEntityId = peerDidEntityId;
        }
    }
}