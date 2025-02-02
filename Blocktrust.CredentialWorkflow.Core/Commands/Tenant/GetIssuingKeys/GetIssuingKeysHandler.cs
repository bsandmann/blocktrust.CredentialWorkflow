using Blocktrust.CredentialWorkflow.Core.Domain.IssuingKey;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Tenant.GetIssuingKeys;

public class GetIssuingKeysHandler : IRequestHandler<GetIssuingKeysRequest, Result<List<IssuingKey>>>
{
    private readonly DataContext _context;

    public GetIssuingKeysHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<List<IssuingKey>>> Handle(GetIssuingKeysRequest request, CancellationToken cancellationToken)
    {
        _context.ChangeTracker.Clear();

        // Fetch all IssuingKeyEntities for the given tenant
        var issuingKeyEntities = await _context.IssuingKeys
            .Where(i => i.TenantEntityId == request.TenantId)
            .ToListAsync(cancellationToken);

        // If none found, we can either return an empty list or consider that a success with empty data
        if (issuingKeyEntities is null || issuingKeyEntities.Count == 0)
        {
            return Result.Ok(new List<IssuingKey>());
        }

        // Map entities to domain models
        var issuingKeys = issuingKeyEntities.Select(i => i.Map(i)).ToList();

        return Result.Ok(issuingKeys);
    }
}