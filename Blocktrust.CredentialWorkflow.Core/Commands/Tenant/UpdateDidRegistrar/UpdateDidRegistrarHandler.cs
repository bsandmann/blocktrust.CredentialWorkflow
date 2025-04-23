namespace Blocktrust.CredentialWorkflow.Core.Commands.Tenant.UpdateDidRegistrar;

using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class UpdateDidRegistrarHandler : IRequestHandler<UpdateDidRegistrarRequest, Result>
{
    private readonly DataContext _context;

    public UpdateDidRegistrarHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateDidRegistrarRequest request, CancellationToken cancellationToken)
    {
        var tenant = await _context.TenantEntities
            .AsTracking()
            .FirstOrDefaultAsync(p => p.TenantEntityId == request.TenantEntityId, cancellationToken);

        if (tenant == null)
        {
            return Result.Fail("Tenant not found");
        }

        // Update the properties
        tenant.OpnRegistrarUrl = request.OpnRegistrarUrl;
        tenant.WalletId = request.WalletId;

        _context.TenantEntities.Update(tenant);

        // Save changes to database
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
