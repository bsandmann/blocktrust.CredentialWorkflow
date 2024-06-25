namespace Blocktrust.CredentialWorkflow.Core.Commands.Tenant.GetTenantInformation;

using Domain.Tenant;
using Domain.Workflow;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetTenantInformationHandler : IRequestHandler<GetTenantInformationRequest, Result<GetTenantInformationResponse>>
{
    private readonly DataContext _context;

    public GetTenantInformationHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<GetTenantInformationResponse>> Handle(GetTenantInformationRequest request, CancellationToken cancellationToken)
    {
        var result = await _context.TenantEntities
            .Select(p =>
                new
                {
                    TenantEntityId = p.TenantEntityId,
                    TenantName = p.Name,
                    ApplicationUserIds = p.ApplicationUsers.Select(q => q.Id),
                    Workflows = p.WorkflowEntities.Select(q =>
                        new
                        {
                            q.WorkflowEntityId, q.Name, q.UpdatedUtc
                        })
                })
            .FirstOrDefaultAsync(p => p.TenantEntityId == request.TenantEntityId, cancellationToken: cancellationToken);
        if (result is null)
        {
            return Result.Fail("Tenant not found");
        }

        return Result.Ok(new GetTenantInformationResponse()
        {
            Tenant = new Tenant()
            {
                Name = result.TenantName,
                TenantId = request.TenantEntityId
            },
            WorkflowSummaries = result.Workflows.Select(p => new WorkflowSummary()
            {
                Name = p.Name,
                UpdatedUtc = p.UpdatedUtc,
                WorkflowId = p.WorkflowEntityId
            }).ToList()
        });
    }
}