namespace Blocktrust.CredentialWorkflow.Core.Commands.WorkflowOutcome.UpdateWorkflowOutcome;

using Blocktrust.CredentialWorkflow.Core.Domain.Enums;
using Domain.Workflow;
using FluentResults;
using MediatR;

public class UpdateWorkflowOutcomeRequest : IRequest<Result<WorkflowOutcome>>
{
    public UpdateWorkflowOutcomeRequest(Guid workflowOutcomeId, EWorkflowOutcomeState workflowOutcomeState, string? outcomeJson, string? errorJson)
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
            ErrorJson = errorJson;
        }
        else if (WorkflowOutcomeState == EWorkflowOutcomeState.NotStarted)
        {
            OutcomeJson = outcomeJson;
        }
    }

    public Guid WorkflowOutcomeId { get; }
    public EWorkflowOutcomeState WorkflowOutcomeState { get; set; }
    public string? ErrorJson { get; set; }
    public string? OutcomeJson { get; set; }
}