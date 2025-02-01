namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.GetWorkflows;

using Blocktrust.CredentialWorkflow.Core.Domain.Workflow;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetWorkflowsHandler : IRequestHandler<GetWorkflowsRequest, Result<List<WorkflowOutcome>>>
{
    private readonly DataContext _context;

    public GetWorkflowsHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<List<WorkflowOutcome>>> Handle(GetWorkflowsRequest request, CancellationToken cancellationToken)
    {
        var workflow = await _context.WorkflowEntities
            .Select(p => new
            {
                p.Name,
                WorkflowId = p.WorkflowEntityId,
                p.UpdatedUtc,
                p.WorkflowState,
                LastOutcome = p.OutcomeEntities.OrderByDescending(q => q.EndedUtc).FirstOrDefault(),
                IsRunable = p.IsRunable
            }).ToListAsync(cancellationToken: cancellationToken);

        if (workflow is null)
        {
            return Result.Fail("The workflow does not exist in the database.");
        }

        var result = workflow.Select(p => new WorkflowOutcome()
        {
            Name = p.Name,
            WorkflowId = p.WorkflowId,
            UpdatedUtc = p.UpdatedUtc,
            WorkflowState = p.WorkflowState,
            LastOutcome = p.LastOutcome?.Map(),
            IsRunable = p.IsRunable
        }).ToList();
        
        return Result.Ok(result);
    }
}