namespace Blocktrust.CredentialWorkflow.Core.Commands.WorkflowOutcome.CreateWorkflowOutcome;

using FluentResults;
using MediatR;

public class CreateWorkflowOutcomeRequest : IRequest<Result<Guid>>
{
    public CreateWorkflowOutcomeRequest(Guid workflowId, string? executionContext)
    {
        WorkflowId = workflowId;
        ExecutionContext = executionContext;
    }

    public Guid WorkflowId { get; }

    public string? ExecutionContext { get; set; }
}