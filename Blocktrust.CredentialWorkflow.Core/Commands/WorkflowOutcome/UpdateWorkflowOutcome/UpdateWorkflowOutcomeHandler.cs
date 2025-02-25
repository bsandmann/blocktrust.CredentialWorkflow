using Blocktrust.CredentialWorkflow.Core.Domain.Enums;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blocktrust.CredentialWorkflow.Core.Commands.WorkflowOutcome.UpdateWorkflowOutcome;

public class UpdateWorkflowOutcomeHandler : IRequestHandler<UpdateWorkflowOutcomeRequest, Result<Domain.Workflow.WorkflowOutcome>>
{
    private readonly DataContext _context;

    public UpdateWorkflowOutcomeHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<Domain.Workflow.WorkflowOutcome>> Handle(UpdateWorkflowOutcomeRequest request, CancellationToken cancellationToken)
    {
        _context.ChangeTracker.Clear();
        var outcomeEntity = await _context.WorkflowOutcomeEntities
            .FirstOrDefaultAsync(o => o.WorkflowOutcomeEntityId == request.WorkflowOutcomeId, cancellationToken);

        if (outcomeEntity is null)
        {
            return Result.Fail<Domain.Workflow.WorkflowOutcome>("The outcome does not exist in the database. The outcome cannot be updated.");
        }
        
        outcomeEntity.WorkflowOutcomeState = request.WorkflowOutcomeState;
        // Ensure EndedUtc updated when state is Success or FailedWithErrors
        if (request.WorkflowOutcomeState == EWorkflowOutcomeState.Success || request.WorkflowOutcomeState == EWorkflowOutcomeState.FailedWithErrors)
        {
            outcomeEntity.EndedUtc = DateTime.UtcNow;
        }
        
        outcomeEntity.ErrorMessage = request.ErrorMessage;
        outcomeEntity.ActionOutcomesJson = request.OutcomeJson;
        
        _context.WorkflowOutcomeEntities.Update(outcomeEntity);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok(outcomeEntity.Map());
    }
}