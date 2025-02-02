using FluentResults;
using MediatR;
using Blocktrust.CredentialWorkflow.Core.Domain.Enums;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Outcome.UpdateOutcomeState
{
    public class UpdateOutcomeStateRequest : IRequest<Result>
    {
        public UpdateOutcomeStateRequest(Guid outcomeId, EOutcomeState newState)
        {
            OutcomeId = outcomeId;
            NewState = newState;
        }

        public Guid OutcomeId { get; }
        public EOutcomeState NewState { get; }
    }
}