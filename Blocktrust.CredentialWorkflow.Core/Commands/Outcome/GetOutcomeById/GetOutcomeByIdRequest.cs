namespace Blocktrust.CredentialWorkflow.Core.Commands.Outcome.GetOutcomeById;

using Blocktrust.CredentialWorkflow.Core.Domain.Outcome;
using FluentResults;
using MediatR;

public class GetOutcomeByIdRequest : IRequest<Result<Outcome>>
{
    public GetOutcomeByIdRequest(Guid outcomeId)
    {
        OutcomeId = outcomeId;
    }

    public Guid OutcomeId { get; }
}