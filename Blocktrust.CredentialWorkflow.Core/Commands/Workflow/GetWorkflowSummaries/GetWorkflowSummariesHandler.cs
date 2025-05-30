namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.GetWorkflowSummaries;

using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow;
using Blocktrust.CredentialWorkflow.Core.Domain.Workflow;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetWorkflowSummariesHandler : IRequestHandler<GetWorkflowSummariesRequest, Result<List<WorkflowSummary>>>
{
    private readonly DataContext _context;

    public GetWorkflowSummariesHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<List<WorkflowSummary>>> Handle(GetWorkflowSummariesRequest request, CancellationToken cancellationToken)
    {
        var workflow = await _context.WorkflowEntities
            .Select(p => new
            {
                p.Name,
                WorkflowId = p.WorkflowEntityId,
                p.UpdatedUtc,
                p.WorkflowState,
                LastOutcome = p.WorkflowOutcomeEntities.OrderByDescending(q => q.EndedUtc).FirstOrDefault(),
                IsRunable = p.IsRunable,
                p.ProcessFlowJson
            }).ToListAsync(cancellationToken: cancellationToken);

        var result = workflow.Select(p => new WorkflowSummary()
        {
            WorkflowId = p.WorkflowId,
            Name = p.Name,
            UpdatedUtc = p.UpdatedUtc,
            WorkflowState = p.WorkflowState,
            LastWorkflowOutcome = p.LastOutcome?.Map(),
            IsRunable = p.IsRunable,
            ProcessFlowJson = p.ProcessFlowJson
        }).ToList();
        
        // Add ProcessFlow by deserializing ProcessFlowJson
        foreach (var summary in result)
        {
            if (!string.IsNullOrEmpty(summary.ProcessFlowJson))
            {
                summary.ProcessFlow = ProcessFlow.DeserializeFromJson(summary.ProcessFlowJson);
            }
        }
        
        return Result.Ok(result);
    }
}