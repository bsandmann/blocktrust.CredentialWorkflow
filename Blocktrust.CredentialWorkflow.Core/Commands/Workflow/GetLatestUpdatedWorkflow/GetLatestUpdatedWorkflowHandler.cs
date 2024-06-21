namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.GetLatestUpdatedWorkflow;

using Domain.Workflow;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetLatestUpdatedWorkflowHandler : IRequestHandler<GetLatestUpdatedWorkflowRequest, Result<Workflow>>
{
    private readonly DataContext _context;

    public GetLatestUpdatedWorkflowHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<Workflow>> Handle(GetLatestUpdatedWorkflowRequest request, CancellationToken cancellationToken)
    {
        var workflow = await _context.WorkflowEntities
            .OrderByDescending(w => w.UpdatedUtc)
            .FirstOrDefaultAsync(p => p.TenantEntityId == request.TenantEntityId, cancellationToken);

        if (workflow is null)
        {
            return Result.Fail("No workflows exist in the database.");
        }

        return Result.Ok(workflow.Map());
    }
}