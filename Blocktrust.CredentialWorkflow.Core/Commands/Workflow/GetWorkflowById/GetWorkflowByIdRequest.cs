namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.GetWorkflowById;

using Domain.Workflow;
using FluentResults;
using MediatR;

public class GetWorkflowByIdRequest : IRequest<Result<Workflow>>
{
    public GetWorkflowByIdRequest(Guid workflowId)
    {
        WorkflowId = workflowId;
    }

    public Guid WorkflowId { get; }
}