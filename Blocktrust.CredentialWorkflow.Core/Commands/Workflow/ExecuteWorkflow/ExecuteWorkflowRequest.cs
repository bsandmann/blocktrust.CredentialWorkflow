namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow;

using Domain.Common;
using Domain.ProcessFlow.Actions;
using Entities.Outcome;
using FluentResults;
using MediatR;

public class ExecuteWorkflowRequest : IRequest<Result<Guid>>
{
    public ExecuteWorkflowRequest(Guid tenantId, ActionOutcome actionOutcome)
    {
        TenantId = tenantId;
        ActionOutcome = actionOutcome;
    }

    public Guid TenantId { get; set; }

    public ActionOutcome ActionOutcome { get; set; }
}