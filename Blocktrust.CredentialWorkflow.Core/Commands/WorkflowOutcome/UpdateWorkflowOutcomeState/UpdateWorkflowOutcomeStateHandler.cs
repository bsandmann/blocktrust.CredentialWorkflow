namespace Blocktrust.CredentialWorkflow.Core.Commands.WorkflowOutcome.UpdateWorkflowOutcomeState;

using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class UpdateWorkflowOutcomeStateHandler : IRequestHandler<UpdateWorkflowOutcomeStateRequest, Result>
{
    private readonly DataContext _context;

    public UpdateWorkflowOutcomeStateHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateWorkflowOutcomeStateRequest request, CancellationToken cancellationToken)
    {
        // Retrieve the outcome from the database
        var outcomeEntity = await _context.WorkflowOutcomeEntities
            .FirstOrDefaultAsync(o => o.WorkflowOutcomeEntityId == request.WorkflowOutcomeId, cancellationToken);

        if (outcomeEntity is null)
        {
            return Result.Fail("The specified outcome does not exist in the database.");
        }

        // Update the state
        outcomeEntity.WorkflowOutcomeState = request.NewState;

        // Save changes to the database
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}