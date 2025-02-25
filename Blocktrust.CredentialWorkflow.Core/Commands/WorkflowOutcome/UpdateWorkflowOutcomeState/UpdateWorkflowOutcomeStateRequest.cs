using Blocktrust.CredentialWorkflow.Core.Domain.Enums;
using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.WorkflowOutcome.UpdateWorkflowOutcomeState;

public class UpdateWorkflowOutcomeStateRequest : IRequest<Result>
{
    public UpdateWorkflowOutcomeStateRequest(Guid workflowOutcomeId, EWorkflowOutcomeState newState)
    {
        WorkflowOutcomeId = workflowOutcomeId;
        NewState = newState;
    }

    public Guid WorkflowOutcomeId { get; }
    public EWorkflowOutcomeState NewState { get; }
}