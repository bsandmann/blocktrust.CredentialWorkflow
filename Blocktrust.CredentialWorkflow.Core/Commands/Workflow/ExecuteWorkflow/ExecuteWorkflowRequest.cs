namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow;

using Entities.Outcome;
using FluentResults;
using MediatR;

public class ExecuteWorkflowRequest : IRequest<Result<Guid>>
{
    public ExecuteWorkflowRequest(OutcomeEntity outcomeEntity)
    {
        OutcomeEntity = outcomeEntity;
    }

    public OutcomeEntity OutcomeEntity { get; set; }
}