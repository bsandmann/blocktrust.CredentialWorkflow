using Blocktrust.CredentialWorkflow.Core.Domain.Enums;
using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.WorkflowOutcome.UpdateWorkflowOutcome;

public class UpdateWorkflowOutcomeRequest : IRequest<Result<Domain.Workflow.WorkflowOutcome>>
{
    public UpdateWorkflowOutcomeRequest(Guid workflowOutcomeId, EWorkflowOutcomeState workflowOutcomeState, string? outcomeJson, string? errorMessage)
    {
        WorkflowOutcomeId = workflowOutcomeId;
        WorkflowOutcomeState = workflowOutcomeState;
        if (WorkflowOutcomeState == EWorkflowOutcomeState.Success)
        {
            OutcomeJson = outcomeJson;
        }
        else if (WorkflowOutcomeState == EWorkflowOutcomeState.FailedWithErrors)
        {
            OutcomeJson = outcomeJson;
            ErrorMessage = errorMessage;
        }
        else if (WorkflowOutcomeState == EWorkflowOutcomeState.NotStarted)
        {
            OutcomeJson = outcomeJson;
        }
    }

    public Guid WorkflowOutcomeId { get; }
    public EWorkflowOutcomeState WorkflowOutcomeState { get; set; }
    public string? ErrorMessage { get; set; }
    public string? OutcomeJson { get; set; }
}