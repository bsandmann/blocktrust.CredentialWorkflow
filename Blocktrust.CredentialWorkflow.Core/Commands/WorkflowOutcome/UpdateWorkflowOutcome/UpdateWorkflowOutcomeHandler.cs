namespace Blocktrust.CredentialWorkflow.Core.Commands.WorkflowOutcome.UpdateWorkflowOutcome;

using Blocktrust.CredentialWorkflow.Core.Domain.Enums;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;
using Domain.Workflow;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class UpdateWorkflowOutcomeHandler : IRequestHandler<UpdateWorkflowOutcomeRequest, Result<WorkflowOutcome>>
{
    private readonly DataContext _context;

    public UpdateWorkflowOutcomeHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<WorkflowOutcome>> Handle(UpdateWorkflowOutcomeRequest request, CancellationToken cancellationToken)
    {
        _context.ChangeTracker.Clear();
        var outcomeEntity = await _context.WorkflowOutcomeEntities
            .FirstOrDefaultAsync(o => o.WorkflowOutcomeEntityId == request.WorkflowOutcomeId, cancellationToken);

        if (outcomeEntity is null)
        {
            return Result.Fail<WorkflowOutcome>("The outcome does not exist in the database. The outcome cannot be updated.");
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