namespace Blocktrust.CredentialWorkflow.Core.Commands.Outcome.UpdateOutcome;

using Domain.Enums;
using Domain.Outcome;
using FluentResults;
using MediatR;

public class UpdateOutcomeRequest : IRequest<Result<Outcome>>
{
    public UpdateOutcomeRequest(Guid outcomeId, EOutcomeState outcomeState, string? outcomeJson, string? errorJson)
    {
        OutcomeId = outcomeId;
        OutcomeState = outcomeState;
        if (OutcomeState == EOutcomeState.Success)
        {
            OutcomeJson = outcomeJson;
        }
        else if (OutcomeState == EOutcomeState.FailedWithErrors)
        {
            OutcomeJson = outcomeJson;
            ErrorJson = errorJson;
        }
        else if (OutcomeState == EOutcomeState.NotStarted)
        {
            OutcomeJson = outcomeJson;
        }
    }

    public Guid OutcomeId { get; }
    public EOutcomeState OutcomeState { get; set; }
    public string? ErrorJson { get; set; }

    public string? OutcomeJson { get; set; }
}