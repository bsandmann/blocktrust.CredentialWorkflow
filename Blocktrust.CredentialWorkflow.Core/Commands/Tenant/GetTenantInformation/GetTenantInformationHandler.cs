namespace Blocktrust.CredentialWorkflow.Core.Commands.Tenant.GetTenantInformation;

using Domain.Tenant;
using Domain.Workflow;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Services;

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
                    WorkflowSummaries = p.WorkflowEntities.Select(q =>
                        new
                        {
                            q.WorkflowEntityId,
                            q.Name,
                            q.UpdatedUtc,
                            q.WorkflowState,
                            WorkflowOutcomeEntity = q.WorkflowOutcomeEntities.OrderByDescending(q => q.EndedUtc).FirstOrDefault()
                        })
                })
            .FirstOrDefaultAsync(p => p.TenantEntityId == request.TenantEntityId, cancellationToken: cancellationToken);
        if (result is null)
        {
            return Result.Fail("Tenant not found");
        }

        var tenantEntity = await _context.TenantEntities
            .FirstOrDefaultAsync(p => p.TenantEntityId == request.TenantEntityId, cancellationToken);

        if (string.IsNullOrEmpty(tenantEntity?.JwtPrivateKey) || string.IsNullOrEmpty(tenantEntity?.JwtPublicKey))
        {
            // Generate new RSA key pair if missing
            var (privateKeyXml, publicKeyXml) = JwtKeyGeneratorService.GenerateRsaKeyPairXml();
            if (tenantEntity != null)
            {
                tenantEntity.JwtPrivateKey = privateKeyXml;
                tenantEntity.JwtPublicKey = publicKeyXml;
                _context.TenantEntities.Update(tenantEntity);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        return Result.Ok(new GetTenantInformationResponse()
        {
            Tenant = new Tenant()
            {
                Name = result.TenantName,
                TenantId = request.TenantEntityId,
                OpnRegistrarUrl = tenantEntity?.OpnRegistrarUrl,
                WalletId = tenantEntity?.WalletId,
                JwtPrivateKey = tenantEntity?.JwtPrivateKey,
                JwtPublicKey = tenantEntity?.JwtPublicKey
            },
            WorkflowSummaries = result.WorkflowSummaries.Select(p => new WorkflowSummary()
            {
                WorkflowId = p.WorkflowEntityId,
                Name = p.Name,
                UpdatedUtc = p.UpdatedUtc,
                WorkflowState = p.WorkflowState,
                LastWorkflowOutcome = p.WorkflowOutcomeEntity?.Map()
            }).ToList()
        });
    }
}