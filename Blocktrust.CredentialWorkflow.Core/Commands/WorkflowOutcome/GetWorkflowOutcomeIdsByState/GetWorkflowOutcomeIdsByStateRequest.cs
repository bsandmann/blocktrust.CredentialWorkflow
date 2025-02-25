namespace Blocktrust.CredentialWorkflow.Core.Commands.WorkflowOutcome.GetWorkflowOutcomeIdsByState;

using Blocktrust.CredentialWorkflow.Core.Domain.Enums;
using FluentResults;
using MediatR;

public class GetWorkflowOutcomeIdsByStateRequest : IRequest<Result<List<GetWorkflowOutcomeIdsByStateResponse>>>
{
    public GetWorkflowOutcomeIdsByStateRequest(IEnumerable<EWorkflowOutcomeState> outcomeStates)
    {
        WorkflowOutcomeStates = outcomeStates?.ToList() ?? new List<EWorkflowOutcomeState>();
    }

    public List<EWorkflowOutcomeState> WorkflowOutcomeStates { get; }
}