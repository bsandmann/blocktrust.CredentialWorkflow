using Blocktrust.CredentialWorkflow.Core.Entities.DIDComm;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.GetPeerDIDs
{
    using Domain.PeerDID;

    public class GetPeerDIDsHandler : IRequestHandler<GetPeerDIDsRequest, Result<List<PeerDIDModel>>>
    {
        private readonly DataContext _context;

        public GetPeerDIDsHandler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<PeerDIDModel>>> Handle(GetPeerDIDsRequest request, CancellationToken cancellationToken)
        {
            _context.ChangeTracker.Clear();

            var peerDIDEntities = await _context.PeerDIDEntities
                .Where(p => p.TenantEntityId == request.TenantId)
                .ToListAsync(cancellationToken);

            // If none found, we can return an empty list as a success
            if (peerDIDEntities is null || peerDIDEntities.Count == 0)
            {
                return Result.Ok(new List<PeerDIDModel>());
            }

            var peerDIDs = peerDIDEntities.Select(p => p.ToModel()).ToList();
            return Result.Ok(peerDIDs);
        }
    }
}