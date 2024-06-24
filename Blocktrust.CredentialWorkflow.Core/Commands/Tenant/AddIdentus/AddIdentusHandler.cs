using Blocktrust.CredentialWorkflow.Core;
using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.AddIdentus;
using Blocktrust.CredentialWorkflow.Core.Entities.Tenant;
using FluentResults;
using MediatR;

public class AddIdentusToTenantHandler : IRequestHandler<AddIdentusToTenantRequest, Result<Guid>>
{
    private readonly DataContext _context;

    public AddIdentusToTenantHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(AddIdentusToTenantRequest request, CancellationToken cancellationToken)
    {
        _context.ChangeTracker.Clear();
        _context.ChangeTracker.AutoDetectChangesEnabled = false;

        var tenant = await _context.TenantEntities.FindAsync(new object[] { request.TenantId }, cancellationToken);

        if (tenant == null)
        {
            return Result.Fail($"Tenant with ID {request.TenantId} not found");
        }

        var identusAgent = new IdentusAgent
        {
            Name = request.Name,
            Uri = request.Uri,
            ApiKey = request.ApiKey,
            TenantId = request.TenantId
        };

        var addedIdentusAgent = await _context.IdentusAgents.AddAsync(identusAgent, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok(addedIdentusAgent.Entity.IdentusAgentId);
    }
}