namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.GetWorkflows;

using Blocktrust.CredentialWorkflow.Core.Domain.Workflow;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetWorkflowsHandler : IRequestHandler<GetWorkflowsRequest, Result<List<WorkflowWithLastResult>>>
{
    private readonly DataContext _context;

    public GetWorkflowsHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<List<WorkflowWithLastResult>>> Handle(GetWorkflowsRequest request, CancellationToken cancellationToken)
    {
        var workflow = await _context.WorkflowEntities
            .Select(p => new
            {
                Name = p.Name,
                WorkflowId = p.WorkflowEntityId,
                UpdatedUtc = p.UpdatedUtc,
                WorkflowState = p.WorkflowState,
                LastOutcome = p.OutcomeEntities.OrderByDescending(q => q.EndedUtc).FirstOrDefault()
            }).ToListAsync(cancellationToken: cancellationToken);

        if (workflow is null)
        {
            return Result.Fail("The workflow does not exist in the database.");
        }

        var result = workflow.Select(p => new WorkflowWithLastResult()
        {
            Name = p.Name,
            WorkflowId = p.WorkflowId,
            UpdatedUtc = p.UpdatedUtc,
            WorkflowState = p.WorkflowState,
            LastOutcome = p.LastOutcome?.Map()
        }).ToList();
        
        return Result.Ok(result);
    }
}