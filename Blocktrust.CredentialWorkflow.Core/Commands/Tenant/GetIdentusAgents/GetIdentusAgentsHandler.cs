using Blocktrust.CredentialWorkflow.Core.Entities.Tenant;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Tenant.GetIdentusAgents;

public class GetIdentusAgentsHandler : IRequestHandler<GetIdentusAgentsRequest, Result<List<IdentusAgent>>>
{
    private readonly DataContext _context;

    public GetIdentusAgentsHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<List<IdentusAgent>>> Handle(GetIdentusAgentsRequest request, CancellationToken cancellationToken)
    {
        var tenant = await _context.TenantEntities
            .Include(t => t.IdentusAgents)
            .FirstOrDefaultAsync(t => t.TenantEntityId == request.TenantId, cancellationToken);

        if (tenant == null)
        {
            return Result.Fail($"Tenant with ID {request.TenantId} not found");
        }

        return Result.Ok(tenant.IdentusAgents.ToList());
    }
}