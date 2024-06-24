using Blocktrust.CredentialWorkflow.Core;
using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.RemoveIdentus;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class RemoveIdentusFromTenantHandler : IRequestHandler<RemoveIdentusFromTenantRequest, Result>
{
    private readonly DataContext _context;

    public RemoveIdentusFromTenantHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(RemoveIdentusFromTenantRequest request, CancellationToken cancellationToken)
    {
        _context.ChangeTracker.Clear();
        _context.ChangeTracker.AutoDetectChangesEnabled = false;

        var identusAgent = await _context.IdentusAgents
            .FirstOrDefaultAsync(i => i.IdentusAgentId == request.IdentusAgentId && i.TenantId == request.TenantId, cancellationToken);

        if (identusAgent == null)
        {
            return Result.Fail($"IdentusAgent with ID {request.IdentusAgentId} not found for Tenant with ID {request.TenantId}");
        }

        _context.IdentusAgents.Remove(identusAgent);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}