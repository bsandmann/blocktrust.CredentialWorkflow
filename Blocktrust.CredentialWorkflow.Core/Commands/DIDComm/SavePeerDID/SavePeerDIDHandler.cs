namespace Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.SavePeerDID
{
    using Blocktrust.CredentialWorkflow.Core.Entities.DIDComm;
    using Domain.PeerDID;
    using FluentResults;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class SavePeerDIDHandler : IRequestHandler<SavePeerDIDRequest, Result<PeerDIDModel>>
    {
        private readonly DataContext _context;

        public SavePeerDIDHandler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<PeerDIDModel>> Handle(SavePeerDIDRequest request, CancellationToken cancellationToken)
        {
            _context.ChangeTracker.Clear();
            _context.ChangeTracker.AutoDetectChangesEnabled = false;

            // Validate tenant
            var tenant = await _context.TenantEntities
                .FirstOrDefaultAsync(t => t.TenantEntityId == request.TenantId, cancellationToken);

            if (tenant is null)
            {
                return Result.Fail("The tenant does not exist in the database. The PeerDID cannot be created.");
            }

            // Create the new PeerDIDEntity
            var peerDIDEntity = new PeerDIDEntity
            {
                PeerDIDEntityId = Guid.NewGuid(),
                Name = request.Name,
                PeerDID = request.PeerDID,
                TenantEntityId = tenant.TenantEntityId,
                CreatedUtc = DateTime.UtcNow
            };

            // Insert and save
            await _context.PeerDIDEntities.AddAsync(peerDIDEntity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // Return the domain model
            return Result.Ok(peerDIDEntity.ToModel());
        }
    }
}