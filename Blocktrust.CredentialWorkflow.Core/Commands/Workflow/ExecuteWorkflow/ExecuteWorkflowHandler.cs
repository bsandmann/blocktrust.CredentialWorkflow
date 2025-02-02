namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow;

using Entities.Outcome;
using FluentResults;
using MediatR;

public class ExecuteWorkflowHandler : IRequestHandler<ExecuteWorkflowRequest, Result<Guid>>
{
    public Task<Result<Guid>> Handle(ExecuteWorkflowRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}