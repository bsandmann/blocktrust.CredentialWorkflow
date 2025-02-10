using Blocktrust.CredentialWorkflow.Core.Entities.DIDComm;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.DeletePeerDID
{
    public class DeletePeerDIDHandler : IRequestHandler<DeletePeerDIDRequest, Result>
    {
        private readonly DataContext _context;

        public DeletePeerDIDHandler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(DeletePeerDIDRequest request, CancellationToken cancellationToken)
        {
            _context.ChangeTracker.Clear();

            var peerDIDEntity = await _context.PeerDIDEntities
                .FirstOrDefaultAsync(p => p.PeerDIDEntityId == request.PeerDIDEntityId, cancellationToken);

            if (peerDIDEntity is null)
            {
                return Result.Fail("The PeerDID does not exist in the database. It cannot be deleted.");
            }

            _context.PeerDIDEntities.Remove(peerDIDEntity);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}