using Blocktrust.CredentialWorkflow.Core;
using Blocktrust.CredentialWorkflow.Core.Domain.IssuingKey;
using Blocktrust.CredentialWorkflow.Core.Entities.Tenant;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class CreateIssuingKeyHandler : IRequestHandler<CreateIssuingKeyRequest, Result<IssuingKey>>
{
    private readonly DataContext _context;

    public CreateIssuingKeyHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<IssuingKey>> Handle(CreateIssuingKeyRequest request, CancellationToken cancellationToken)
    {
        _context.ChangeTracker.Clear();
        _context.ChangeTracker.AutoDetectChangesEnabled = false;

        var tenant = await _context.TenantEntities
            .FirstOrDefaultAsync(t => t.TenantEntityId == request.TenantId, cancellationToken);

        if (tenant is null)
        {
            return Result.Fail("The tenant does not exist in the database. The IssuingKey cannot be created.");
        }

        var issuingKey = new IssuingKeyEntity
        {
            IssuingKeyId = Guid.NewGuid(),
            Name = request.Name,
            CreatedUtc = DateTime.UtcNow,
            KeyType = request.KeyType,
            PublicKey = request.PublicKey,
            PrivateKey = request.PrivateKey
        };

        issuingKey.TenantEntityId = tenant.TenantEntityId;

        await _context.IssuingKeys.AddAsync(issuingKey, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok(issuingKey.Map(issuingKey));
    }
}