namespace Blocktrust.CredentialWorkflow.Core.Commands.WorkflowOutcome.GetWorkflowOutcomeIdsByState
{
    using Blocktrust.CredentialWorkflow.Core.Domain.Enums;

    public class GetWorkflowOutcomeIdsByStateResponse
    {
        public Guid OutcomeId { get; set; }
        public EWorkflowOutcomeState WorkflowOutcomeState { get; set; }
    }
}