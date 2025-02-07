namespace Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.SavePeerDID
{
    using Domain.PeerDID;
    using FluentResults;
    using MediatR;

    public class SavePeerDIDRequest : IRequest<Result<PeerDIDModel>>
    {
        public SavePeerDIDRequest(Guid tenantId, string name, string peerDid)
        {
            TenantId = tenantId;
            Name = name;
            PeerDID = peerDid;
        }

        public Guid TenantId { get; }
        public string Name { get; }
        public string PeerDID { get; }
    }
}