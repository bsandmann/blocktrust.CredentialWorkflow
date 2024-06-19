namespace Blocktrust.CredentialWorkflow.Core.Commands.GetTenantInformation;

using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Models;

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
        });
    }
}