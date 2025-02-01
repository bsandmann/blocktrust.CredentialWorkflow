namespace Blocktrust.CredentialWorkflow.Core.Commands.Outcome.CreateOutcome;

using FluentResults;
using MediatR;
using Blocktrust.CredentialWorkflow.Core.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Entities.Outcome;

public class CreateOutcomeHandler : IRequestHandler<CreateOutcomeRequest, Result<Guid>>
{
    private readonly DataContext _context;

    public CreateOutcomeHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateOutcomeRequest request, CancellationToken cancellationToken)
    {
        _context.ChangeTracker.Clear();

        var workflow = await _context.WorkflowEntities
            .FirstOrDefaultAsync(w => w.WorkflowEntityId == request.WorkflowId, cancellationToken);

        if (workflow is null)
        {
            return Result.Fail<Guid>("The workflow does not exist in the database. The outcome cannot be created.");
        }

        var outcomeEntity = new OutcomeEntity()
        {
            OutcomeState = EOutcomeState.NotStarted,
            WorkflowEntityId = workflow.WorkflowEntityId,
            StartedUtc = DateTime.UtcNow,
            ExecutionContext = request.ExecutionContext
        };

        await _context.OutcomeEntities.AddAsync(outcomeEntity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok(outcomeEntity.OutcomeEntityId);
    }
}