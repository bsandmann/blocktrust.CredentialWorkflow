using Blocktrust.CredentialWorkflow.Core.Domain.Enums;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Outcome.GetOutcomeIdsByState
{
    public class GetOutcomeIdsByStateResponse
    {
        public Guid OutcomeId { get; set; }
        public EOutcomeState OutcomeState { get; set; }
    }
}