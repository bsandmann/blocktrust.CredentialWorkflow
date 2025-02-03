namespace Blocktrust.CredentialWorkflow.Core.Commands.WorkflowOutcome.GetWorkflowOutcomeById;

using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;
using Domain.Workflow;
using FluentResults;
using MediatR;

public class GetWorkflowOutcomeByIdRequest : IRequest<Result<WorkflowOutcome>>
{
    public GetWorkflowOutcomeByIdRequest(Guid workflowOutcomeId)
    {
        WorkflowOutcomeId = workflowOutcomeId;
    }

    public Guid WorkflowOutcomeId { get; }
}