namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.GetWorkflows;

using Blocktrust.CredentialWorkflow.Core.Domain.Workflow;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetWorkflowsHandler : IRequestHandler<GetWorkflowsRequest, Result<List<Workflow>>>
{
    private readonly DataContext _context;

    public GetWorkflowsHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<List<Workflow>>> Handle(GetWorkflowsRequest request, CancellationToken cancellationToken)
    {
        var workflows = await _context.WorkflowEntities
            .AsNoTracking()
            .Include(w => w.WorkflowOutcomeEntities)
            .Select(w => w.Map())
            .ToListAsync(cancellationToken);

        return Result.Ok(workflows);
    }
}