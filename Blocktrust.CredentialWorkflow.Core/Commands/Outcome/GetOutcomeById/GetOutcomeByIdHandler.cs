namespace Blocktrust.CredentialWorkflow.Core.Commands.Outcome.GetOutcomeById;

using Blocktrust.CredentialWorkflow.Core.Domain.Outcome;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetOutcomeByIdHandler : IRequestHandler<GetOutcomeByIdRequest, Result<Outcome>>
{
    private readonly DataContext _context;

    public GetOutcomeByIdHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<Outcome>> Handle(GetOutcomeByIdRequest request, CancellationToken cancellationToken)
    {
        var outcomeEntity = await _context.OutcomeEntities
            .Include(o => o.WorkflowEntityId) // include related workflow if required
            .FirstOrDefaultAsync(o => o.OutcomeEntityId == request.OutcomeId, cancellationToken);

        if (outcomeEntity is null)
        {
            return Result.Fail("The outcome does not exist in the database.");
        }

        return Result.Ok(outcomeEntity.Map());
    }
}