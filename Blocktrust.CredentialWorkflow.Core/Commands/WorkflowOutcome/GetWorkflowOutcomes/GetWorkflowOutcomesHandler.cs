namespace Blocktrust.CredentialWorkflow.Core.Commands.WorkflowOutcome.GetWorkflowOutcomes;

using Blocktrust.CredentialWorkflow.Core.Domain.Workflow;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetWorkflowOutcomesHandler : IRequestHandler<GetWorkflowOutcomesRequest, Result<List<WorkflowOutcome>>>
{
    private readonly DataContext _context;

    public GetWorkflowOutcomesHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<List<WorkflowOutcome>>> Handle(GetWorkflowOutcomesRequest request, CancellationToken cancellationToken)
    {
        var workflowOutcomeEntities = await _context.WorkflowOutcomeEntities
            .Include(o => o.WorkflowEntity)
            .Where(o => o.WorkflowEntityId == request.WorkflowId)
            .ToListAsync(cancellationToken: cancellationToken);

        return Result.Ok(workflowOutcomeEntities.Select(p => p.Map()).ToList());
    }
}