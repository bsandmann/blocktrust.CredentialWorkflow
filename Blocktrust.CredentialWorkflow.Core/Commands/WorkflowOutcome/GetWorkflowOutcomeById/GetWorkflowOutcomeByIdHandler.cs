namespace Blocktrust.CredentialWorkflow.Core.Commands.WorkflowOutcome.GetWorkflowOutcomeById;

using Domain.Workflow;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetWorkflowOutcomeByIdHandler : IRequestHandler<GetWorkflowOutcomeByIdRequest, Result<WorkflowOutcome>>
{
    private readonly DataContext _context;

    public GetWorkflowOutcomeByIdHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<WorkflowOutcome>> Handle(GetWorkflowOutcomeByIdRequest request, CancellationToken cancellationToken)
    {
        var workflowOutcomeEntity = await _context.WorkflowOutcomeEntities
            .Include(p => p.WorkflowEntity)
            .FirstOrDefaultAsync(o => o.WorkflowOutcomeEntityId == request.WorkflowOutcomeId, cancellationToken);

        if (workflowOutcomeEntity is null)
        {
            return Result.Fail("The outcome does not exist in the database.");
        }

        return Result.Ok(workflowOutcomeEntity.Map());
    }
}