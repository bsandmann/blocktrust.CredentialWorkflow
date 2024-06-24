namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.UpdateWorkflow;

using System.Text.Json;
using Domain.Workflow;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class UpdateWorkflowHandler : IRequestHandler<UpdateWorkflowRequest, Result<Workflow>>
{
    private readonly DataContext _context;

    public UpdateWorkflowHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<Workflow>> Handle(UpdateWorkflowRequest request, CancellationToken cancellationToken)
    {
        _context.ChangeTracker.Clear();

        var workflow = await _context.WorkflowEntities
            .FirstOrDefaultAsync(w => w.WorkflowEntityId == request.WorkflowId, cancellationToken);

        if (workflow is null)
        {
            return Result.Fail("The workflow does not exist in the database. It cannot be updated.");
        }

        workflow.UpdatedUtc = DateTime.UtcNow;
        workflow.Name = request.Name;
        workflow.WorkflowState = request.WorkflowState;
        if (request.ProcessFlow is not null)
        {
            workflow.ProcessFlowJson = request.ProcessFlow.SerializeToJson();
        }

        _context.WorkflowEntities.Update(workflow);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok(workflow.Map());
    }
}