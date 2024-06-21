namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.GetLatestUpdatedWorkflow;

using Blocktrust.CredentialWorkflow.Core.Entities.Workflow;
using Domain.Workflow;
using FluentResults;
using MediatR;

public class GetLatestUpdatedWorkflowRequest : IRequest<Result<Workflow>>
{
    public GetLatestUpdatedWorkflowRequest(Guid tenantEntityId)
    {
        TenantEntityId = tenantEntityId;
    }

    public Guid TenantEntityId { get; set; }
}