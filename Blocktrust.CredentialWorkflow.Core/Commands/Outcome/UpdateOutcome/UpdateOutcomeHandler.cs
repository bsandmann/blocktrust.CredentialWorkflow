using Blocktrust.CredentialWorkflow.Core.Domain.Common;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Outcome.UpdateOutcome;

using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Domain.Enums;
using Domain.ProcessFlow.Actions;

public class UpdateOutcomeHandler : IRequestHandler<UpdateOutcomeRequest, Result<ActionOutcome>>
{
    private readonly DataContext _context;

    public UpdateOutcomeHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<ActionOutcome>> Handle(UpdateOutcomeRequest request, CancellationToken cancellationToken)
    {
        _context.ChangeTracker.Clear();
        var outcomeEntity = await _context.OutcomeEntities
            .FirstOrDefaultAsync(o => o.OutcomeEntityId == request.OutcomeId, cancellationToken);

        if (outcomeEntity is null)
        {
            return Result.Fail<ActionOutcome>("The outcome does not exist in the database. The outcome cannot be updated.");
        }
        
        outcomeEntity.OutcomeState = request.OutcomeState;
        // Ensure EndedUtc updated when state is Success or FailedWithErrors
        if (request.OutcomeState == EOutcomeState.Success || request.OutcomeState == EOutcomeState.FailedWithErrors)
        {
            outcomeEntity.EndedUtc = DateTime.UtcNow;
        }
        
        outcomeEntity.ErrorJson = request.ErrorJson;
        outcomeEntity.OutcomeJson = request.OutcomeJson;
        
        _context.OutcomeEntities.Update(outcomeEntity);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok(outcomeEntity.Map());
    }
}