using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Blocktrust.CredentialWorkflow.Core.Domain.Enums;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Outcome.UpdateOutcomeState
{
    public class UpdateOutcomeStateHandler : IRequestHandler<UpdateOutcomeStateRequest, Result>
    {
        private readonly DataContext _context;

        public UpdateOutcomeStateHandler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(UpdateOutcomeStateRequest request, CancellationToken cancellationToken)
        {
            // Retrieve the outcome from the database
            var outcomeEntity = await _context.OutcomeEntities
                .FirstOrDefaultAsync(o => o.OutcomeEntityId == request.OutcomeId, cancellationToken);

            if (outcomeEntity is null)
            {
                return Result.Fail("The specified outcome does not exist in the database.");
            }

            // Update the state
            outcomeEntity.OutcomeState = request.NewState;

            // Optionally update any relevant timestamps, if desired
            // e.g., outcomeEntity.LastUpdatedUtc = DateTime.UtcNow;

            // Save changes to the database
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}