using FluentResults;
using MediatR;
using Blocktrust.CredentialWorkflow.Core.Domain.Enums;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Outcome.GetOutcomeIdsByState
{
    public class GetOutcomeIdsByStateRequest : IRequest<Result<List<GetOutcomeIdsByStateResponse>>>
    {
        public GetOutcomeIdsByStateRequest(IEnumerable<EOutcomeState> outcomeStates)
        {
            OutcomeStates = outcomeStates?.ToList() ?? new List<EOutcomeState>();
        }

        public List<EOutcomeState> OutcomeStates { get; }
    }
}