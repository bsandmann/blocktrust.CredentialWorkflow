namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.GetWorkflows;

using Blocktrust.CredentialWorkflow.Core.Domain.Workflow;
using FluentResults;
using MediatR;

public class GetWorkflowsRequest : IRequest<Result<List<Workflow>>>
{
    public GetWorkflowsRequest(Guid tenantEntityId)
    {
        TenantEntityId = tenantEntityId;
    }

    public Guid TenantEntityId { get; }
}