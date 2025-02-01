namespace Blocktrust.CredentialWorkflow.Core.Commands.Outcome.CreateOutcome;

using FluentResults;
using MediatR;
using Blocktrust.CredentialWorkflow.Core.Domain.Enums;

public class CreateOutcomeRequest : IRequest<Result<Guid>>
{
    public CreateOutcomeRequest(Guid workflowId, string? executionContext)
    {
        WorkflowId = workflowId;
        ExecutionContext = executionContext;
    }

    public Guid WorkflowId { get; }

    public string? ExecutionContext { get; set; }
}