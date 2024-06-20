namespace Blocktrust.CredentialWorkflow.Core.Commands.Outcome.CreateOutcome;

using FluentResults;
using MediatR;
using Blocktrust.CredentialWorkflow.Core.Domain.Enums;

public class CreateOutcomeRequest : IRequest<Result<Guid>>
{
    public CreateOutcomeRequest(Guid workflowId)
    {
        WorkflowId = workflowId;
    }

    public Guid WorkflowId { get; }
}