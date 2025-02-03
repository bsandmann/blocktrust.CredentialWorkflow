namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow;

using Domain.Common;
using Domain.ProcessFlow.Actions;
using Domain.Workflow;
using Entities.Outcome;
using FluentResults;
using MediatR;

public class ExecuteWorkflowRequest : IRequest<Result<bool>>
{
    public ExecuteWorkflowRequest(WorkflowOutcome workflowOutcome)
    {
        WorkflowOutcome = workflowOutcome;
    }


    public WorkflowOutcome WorkflowOutcome { get; set; }
}