namespace Blocktrust.CredentialWorkflow.Core.Commands.Tenant.DeleteTenant;

using Entities.Tenant;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class DeleteTenantHandler : IRequestHandler<DeleteTenantRequest, Result>
{
    private readonly DataContext _context;
    private readonly IMediator _mediator;

    public DeleteTenantHandler(DataContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<Result> Handle(DeleteTenantRequest request, CancellationToken cancellationToken)
    {
        _context.ChangeTracker.Clear();
        _context.ChangeTracker.AutoDetectChangesEnabled = false;

        var tenant = await _context.TenantEntities
            .Select(p => new { p.TenantEntityId })
            .FirstOrDefaultAsync(p => p.TenantEntityId == request.TenantId, cancellationToken);

        if (tenant is null)
        {
            return Result.Fail("Tenant not found");
        }

        // First delete all the pools associated with the tenant

        _context.TenantEntities.Remove(new TenantEntity
        {
            TenantEntityId = tenant.TenantEntityId,
            Name = null!
        }); 
        
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}