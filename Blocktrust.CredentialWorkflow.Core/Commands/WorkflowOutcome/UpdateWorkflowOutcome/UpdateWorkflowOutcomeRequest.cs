namespace Blocktrust.CredentialWorkflow.Core.Commands.WorkflowOutcome.UpdateWorkflowOutcome;

using Blocktrust.CredentialWorkflow.Core.Domain.Enums;
using Domain.Workflow;
using FluentResults;
using MediatR;

public class UpdateWorkflowOutcomeRequest : IRequest<Result<WorkflowOutcome>>
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