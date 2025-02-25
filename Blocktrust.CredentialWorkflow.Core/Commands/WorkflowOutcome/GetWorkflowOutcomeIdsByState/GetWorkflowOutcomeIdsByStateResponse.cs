namespace Blocktrust.CredentialWorkflow.Core.Commands.WorkflowOutcome.GetWorkflowOutcomeIdsByState;

using Domain.Enums;

public class GetWorkflowOutcomeIdsByStateResponse
{
    public Guid OutcomeId { get; set; }
    public EWorkflowOutcomeState WorkflowOutcomeState { get; set; }
}