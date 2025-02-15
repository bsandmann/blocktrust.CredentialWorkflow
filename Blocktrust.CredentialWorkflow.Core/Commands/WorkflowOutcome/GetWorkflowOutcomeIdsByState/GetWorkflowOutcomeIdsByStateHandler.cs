namespace Blocktrust.CredentialWorkflow.Core.Commands.WorkflowOutcome.GetWorkflowOutcomeIdsByState;

using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetWorkflowOutcomeIdsByStateHandler
    : IRequestHandler<GetWorkflowOutcomeIdsByStateRequest, Result<List<GetWorkflowOutcomeIdsByStateResponse>>>
{
    private readonly DataContext _context;

    public GetWorkflowOutcomeIdsByStateHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<List<GetWorkflowOutcomeIdsByStateResponse>>> Handle(
        GetWorkflowOutcomeIdsByStateRequest request,
        CancellationToken cancellationToken)
    {
        var outcomes = await _context.WorkflowOutcomeEntities
            .Where(o => request.WorkflowOutcomeStates.Contains(o.WorkflowOutcomeState))
            .Select(o => new GetWorkflowOutcomeIdsByStateResponse
            {
                OutcomeId = o.WorkflowOutcomeEntityId,
                WorkflowOutcomeState = o.WorkflowOutcomeState
            })
            .ToListAsync(cancellationToken);

        return Result.Ok(outcomes);
    }
}