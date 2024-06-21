namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.GetWorkflowById;

using Blocktrust.CredentialWorkflow.Core.Entities.Workflow;
using Domain.Workflow;
using FluentResults;
using MediatR;

public class GetWorkflowByIdRequest : IRequest<Result<Workflow>>
{
    public GetWorkflowByIdRequest(Guid workflowId, Guid tenantEntityId)
    {
        WorkflowId = workflowId;
        TenantEntityId = tenantEntityId;
    }

    public Guid WorkflowId { get; }
    public Guid TenantEntityId { get; }
}