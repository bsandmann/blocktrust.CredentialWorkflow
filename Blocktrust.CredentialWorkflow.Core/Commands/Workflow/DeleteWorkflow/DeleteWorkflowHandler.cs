namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.DeleteWorkflow;

using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class DeleteWorkflowHandler : IRequestHandler<DeleteWorkflowRequest, Result>
{
    private readonly DataContext _context;

    public DeleteWorkflowHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteWorkflowRequest request, CancellationToken cancellationToken)
    {
        _context.ChangeTracker.Clear();

        var workflow = await _context.WorkflowEntities
            .FirstOrDefaultAsync(w => w.WorkflowEntityId == request.WorkflowId, cancellationToken);

        if (workflow is null)
        {
            return Result.Fail("The workflow does not exist in the database. It cannot be deleted.");
        }

        _context.WorkflowEntities.Remove(workflow);
        
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}