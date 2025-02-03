namespace Blocktrust.CredentialWorkflow.Core.Commands.WorkflowOutcome.GetWorkflowOutcomes;

using Blocktrust.CredentialWorkflow.Core.Domain.Workflow;
using FluentResults;
using MediatR;

public class GetWorkflowOutcomesRequest : IRequest<Result<List<WorkflowOutcome>>>
{
    public GetWorkflowOutcomesRequest(Guid workflowId)
    {
        WorkflowId = workflowId;
    }

    public Guid WorkflowId { get; }
}