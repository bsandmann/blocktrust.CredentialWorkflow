namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.DeleteWorkflow;

using FluentResults;
using MediatR;

public class DeleteWorkflowRequest : IRequest<Result>
{
    public DeleteWorkflowRequest(Guid workflowId)
    {
        WorkflowId = workflowId;
    }

    public Guid WorkflowId { get; }
}