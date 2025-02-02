using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Blocktrust.CredentialWorkflow.Core.Domain.Enums;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Outcome.GetOutcomeIdsByState
{
    public class GetOutcomeIdsByStateHandler : IRequestHandler<GetOutcomeIdsByStateRequest, Result<List<Guid>>>
    {
        private readonly DataContext _context;

        public GetOutcomeIdsByStateHandler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<Guid>>> Handle(GetOutcomeIdsByStateRequest request, CancellationToken cancellationToken)
        {
            // Filter by any of the states in request.OutcomeStates
            var outcomeIds = await _context.OutcomeEntities
                .Where(o => request.OutcomeStates.Contains(o.OutcomeState))
                .Select(o => o.OutcomeEntityId)
                .ToListAsync(cancellationToken);

            return Result.Ok(outcomeIds);
        }
    }
}