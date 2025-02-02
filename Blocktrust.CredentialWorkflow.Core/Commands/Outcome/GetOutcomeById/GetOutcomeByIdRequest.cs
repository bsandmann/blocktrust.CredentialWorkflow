using Blocktrust.CredentialWorkflow.Core.Domain.Common;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Outcome.GetOutcomeById;

using Domain.ProcessFlow.Actions;
using FluentResults;
using MediatR;

public class GetOutcomeByIdRequest : IRequest<Result<ActionOutcome>>
{
    public GetOutcomeByIdRequest(Guid outcomeId)
    {
        OutcomeId = outcomeId;
    }

    public Guid OutcomeId { get; }
}