namespace Blocktrust.CredentialWorkflow.Core.Commands.GetTenantInformation;

using FluentResults;
using MediatR;

public class GetTenantInformationRequest : IRequest<Result<GetTenantInformationResponse>>
{
    public GetTenantInformationRequest(Guid tenantEntityId)
    {
        TenantEntityId = tenantEntityId;
    }
    
    public Guid TenantEntityId { get; set; } 
}