namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ChangeWorkflowState
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Domain.Enums;
    using Domain.Workflow;
    using FluentResults;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class ChangeWorkflowStateHandler : IRequestHandler<ChangeWorkflowStateRequest, Result<Workflow>>
    {
        private readonly DataContext _context;

        public ChangeWorkflowStateHandler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<Workflow>> Handle(ChangeWorkflowStateRequest request, CancellationToken cancellationToken)
        {
            _context.ChangeTracker.Clear();

            var workflow = await _context.WorkflowEntities
                .FirstOrDefaultAsync(w => w.WorkflowEntityId == request.WorkflowId, cancellationToken);

            if (workflow is null)
            {
                return Result.Fail("The workflow does not exist in the database. It cannot change its state.");
            }

            workflow.WorkflowState = request.WorkflowState;
            workflow.UpdatedUtc = DateTime.UtcNow;
            if (workflow.WorkflowState != EWorkflowState.Inactive)
            {
                workflow.IsRunable = true;
            }

            _context.WorkflowEntities.Update(workflow);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Ok(workflow.Map());
        }
    }
}