namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.GetWorkflowById;

using Domain.Workflow;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetWorkflowByIdHandler : IRequestHandler<GetWorkflowByIdRequest, Result<Workflow>>
{
    private readonly DataContext _context;

    public GetWorkflowByIdHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<Workflow>> Handle(GetWorkflowByIdRequest request, CancellationToken cancellationToken)
    {
        var workflow = await _context.WorkflowEntities
            .FirstOrDefaultAsync(w => w.WorkflowEntityId == request.WorkflowId, cancellationToken);

        if (workflow is null)
        {
            return Result.Fail("The workflow does not exist in the database.");
        }

        return Result.Ok(workflow.Map());
    }
}