using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.DeletePeerDID
{
    public class DeletePeerDIDRequest : IRequest<Result>
    {
        public DeletePeerDIDRequest(Guid peerDIDEntityId)
        {
            PeerDIDEntityId = peerDIDEntityId;
        }

        public Guid PeerDIDEntityId { get; }
    }
}