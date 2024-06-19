namespace Blocktrust.CredentialWorkflow.Core.Commands.CreateTenant;

using Entities.Tenants;
using FluentResults;
using MediatR;

public class CreateTenantHandler : IRequestHandler<CreateTenantRequest, Result<Guid>>
{
    private readonly DataContext _context;

    public CreateTenantHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateTenantRequest request, CancellationToken cancellationToken)
    {
        _context.ChangeTracker.Clear();
        _context.ChangeTracker.AutoDetectChangesEnabled = false;

        if (string.IsNullOrEmpty(request.Name))
        {
            return Result.Fail("The tenant name must be provided");
        }

        var tenant = await _context.TenantEntities.AddAsync(new TenantEntity()
        {
            Name = request.Name,
            CreatedUtc = DateTime.UtcNow
        }, cancellationToken: cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok(tenant.Entity.TenantEntityId);
    }
}