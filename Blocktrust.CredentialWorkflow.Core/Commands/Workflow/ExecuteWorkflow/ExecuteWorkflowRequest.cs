namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow;

using FluentResults;
using MediatR;

public class ExecuteWorkflowRequest : IRequest<Result<bool>>
{
    public Guid WorkflowEntity { get; set; }
}