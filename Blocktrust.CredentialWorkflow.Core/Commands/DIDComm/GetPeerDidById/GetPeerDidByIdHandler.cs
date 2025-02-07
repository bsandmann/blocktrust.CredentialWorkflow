using Blocktrust.CredentialWorkflow.Core.Entities.DIDComm;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.GetPeerDidById
{
    using Domain.PeerDID;

    public class GetPeerDidByIdHandler : IRequestHandler<GetPeerDidByIdRequest, Result<PeerDIDModel>>
    {
        private readonly DataContext _context;

        public GetPeerDidByIdHandler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<PeerDIDModel>> Handle(GetPeerDidByIdRequest request, CancellationToken cancellationToken)
        {
            _context.ChangeTracker.Clear();

            var peerDIDEntity = await _context.PeerDIDEntities
                .FirstOrDefaultAsync(p => p.PeerDIDEntityId == request.PeerDidEntityId, cancellationToken);

            if (peerDIDEntity is null)
            {
                return Result.Fail($"PeerDID with ID '{request.PeerDidEntityId}' not found.");
            }

            return Result.Ok(peerDIDEntity.ToModel());
        }
    }
}