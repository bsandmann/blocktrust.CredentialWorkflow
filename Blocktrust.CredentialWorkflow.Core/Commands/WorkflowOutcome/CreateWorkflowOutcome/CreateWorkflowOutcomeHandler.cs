namespace Blocktrust.CredentialWorkflow.Core.Commands.WorkflowOutcome.CreateWorkflowOutcome;

using Blocktrust.CredentialWorkflow.Core.Domain.Enums;
using Blocktrust.CredentialWorkflow.Core.Entities.Outcome;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class CreateWorkflowOutcomeHandler : IRequestHandler<CreateWorkflowOutcomeRequest, Result<Guid>>
{
    private readonly DataContext _context;

    public CreateWorkflowOutcomeHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateWorkflowOutcomeRequest request, CancellationToken cancellationToken)
    {
        _context.ChangeTracker.Clear();

        var workflow = await _context.WorkflowEntities
            .FirstOrDefaultAsync(w => w.WorkflowEntityId == request.WorkflowId, cancellationToken);

        if (workflow is null)
        {
            return Result.Fail<Guid>("The workflow does not exist in the database. The outcome cannot be created.");
        }

        var outcomeEntity = new WorkflowOutcomeEntity()
        {
            WorkflowOutcomeState = EWorkflowOutcomeState.NotStarted,
            WorkflowEntityId = workflow.WorkflowEntityId,
            StartedUtc = DateTime.UtcNow,
            ExecutionContext = request.ExecutionContext
        };

        await _context.WorkflowOutcomeEntities.AddAsync(outcomeEntity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok(outcomeEntity.WorkflowOutcomeEntityId);
    }
}