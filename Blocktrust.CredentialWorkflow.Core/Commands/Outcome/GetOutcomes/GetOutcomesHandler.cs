using Blocktrust.CredentialWorkflow.Core.Domain.Common;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Outcome.GetOutcomes;

using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetOutcomesHandler : IRequestHandler<GetOutcomesRequest, Result<List<ActionOutcome>>>
{
    private readonly DataContext _context;

    public GetOutcomesHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ActionOutcome>>> Handle(GetOutcomesRequest request, CancellationToken cancellationToken)
    {
        var outcomeEntities = await _context.OutcomeEntities
            .Include(o => o.WorkflowEntity) // include related workflow if require
            .Where(o => o.WorkflowEntityId == request.WorkflowId)
            .ToListAsync(cancellationToken: cancellationToken);

        return Result.Ok(outcomeEntities.Select(p => p.Map()).ToList());
    }
}