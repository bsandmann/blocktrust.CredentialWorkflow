using Blocktrust.CredentialWorkflow.Core.Entities.Workflow;
using Blocktrust.CredentialWorkflow.Core.Domain.Enums;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.GetActiveRecurringWorkflows
{
    using Domain.ProcessFlow;
    using Domain.ProcessFlow.Triggers;
    using Domain.Workflow;

    public class GetActiveRecurringWorkflowsHandler : IRequestHandler<GetActiveRecurringWorkflowsRequest, Result<List<ActiveRecurringWorkflow>>>
    {
        private readonly DataContext _context;

        public GetActiveRecurringWorkflowsHandler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<ActiveRecurringWorkflow>>> Handle(GetActiveRecurringWorkflowsRequest request, CancellationToken cancellationToken)
        {
            // Query all workflows that are active with recurrent triggers.
            var workflows = await _context.WorkflowEntities
                .Where(w => w.WorkflowState == EWorkflowState.ActiveWithRecurrentTrigger && w.ProcessFlowJson != null)
                .ToListAsync(cancellationToken);

            if (!workflows.Any())
            {
                return Result.Ok(new List<ActiveRecurringWorkflow>());
            }

            var activeRecurringWorkflows = workflows.Select(w => new ActiveRecurringWorkflow
                {
                    WorkflowEntityId = w.WorkflowEntityId,
                    WorkflowState = w.WorkflowState,
                    UpdatedUtc = w.UpdatedUtc,
                    CronExpression = ExtractCronExpression(w.ProcessFlowJson)
                }).Where(p => !string.IsNullOrEmpty(p.CronExpression))
                .ToList();

            return Result.Ok(activeRecurringWorkflows);
        }

        private string ExtractCronExpression(string? processFlowJson)
        {
            var stringValue = string.IsNullOrEmpty(processFlowJson) ? string.Empty : processFlowJson;
            var processFlow = ProcessFlow.DeserializeFromJson(stringValue);
            var trigger = processFlow.Triggers.First().Value;
            if (trigger.Type == ETriggerType.RecurringTimer)
            {
                var recurringTimer = (TriggerInputRecurringTimer)trigger.Input;
                if (string.IsNullOrEmpty(recurringTimer.CronExpression))
                {
                    return string.Empty;
                }

                return recurringTimer.CronExpression;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}