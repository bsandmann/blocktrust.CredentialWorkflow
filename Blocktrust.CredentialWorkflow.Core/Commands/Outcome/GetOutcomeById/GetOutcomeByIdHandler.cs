using Blocktrust.CredentialWorkflow.Core.Domain.Common;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Outcome.GetOutcomeById;

using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetOutcomeByIdHandler : IRequestHandler<GetOutcomeByIdRequest, Result<ActionOutcome>>
{
    private readonly DataContext _context;

    public GetOutcomeByIdHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<ActionOutcome>> Handle(GetOutcomeByIdRequest request, CancellationToken cancellationToken)
    {
        var outcomeEntity = await _context.OutcomeEntities
            .FirstOrDefaultAsync(o => o.OutcomeEntityId == request.OutcomeId, cancellationToken);

        if (outcomeEntity is null)
        {
            return Result.Fail("The outcome does not exist in the database.");
        }

        return Result.Ok(outcomeEntity.Map());
    }
}