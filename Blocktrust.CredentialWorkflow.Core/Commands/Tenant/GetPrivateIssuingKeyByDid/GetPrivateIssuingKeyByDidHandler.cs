using Blocktrust.CredentialWorkflow.Core.Entities.Tenant;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Tenant.GetPrivateIssuingKeyByDid
{
    public class GetPrivateIssuingKeyByDidHandler : IRequestHandler<GetPrivateIssuingKeyByDidRequest, Result<string>>
    {
        private readonly DataContext _context;

        public GetPrivateIssuingKeyByDidHandler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<string>> Handle(GetPrivateIssuingKeyByDidRequest request, CancellationToken cancellationToken)
        {
            // Clear EF's change tracker (optional, depending on how you track concurrency).
            _context.ChangeTracker.Clear();

            // Locate the IssuingKeyEntity by DID
            var issuingKeyEntity = await _context.IssuingKeys
                .FirstOrDefaultAsync(i => i.Did == request.Did, cancellationToken);

            if (issuingKeyEntity is null)
            {
                return Result.Fail("Issuing key not found for the provided DID.");
            }

            // Return just the private key string
            var privateKey = issuingKeyEntity.PrivateKey;
            return Result.Ok(privateKey);
        }
    }
}