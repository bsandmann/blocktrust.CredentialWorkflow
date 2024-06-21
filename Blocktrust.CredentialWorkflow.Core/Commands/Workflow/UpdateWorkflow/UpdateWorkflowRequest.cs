namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.UpdateWorkflow;

using FluentResults;
using MediatR;
using Domain.Enums;
using Domain.ProcessFlow;
using Domain.Workflow;

public class UpdateWorkflowRequest : IRequest<Result<Workflow>>
{
    public UpdateWorkflowRequest(Guid workflowId, string name, EWorkflowState workflowState, ProcessFlow? processFlow)
    {
        WorkflowId = workflowId;
        Name = name;
        WorkflowState = workflowState;
        ProcessFlow = processFlow;
    }

    public Guid WorkflowId { get; }
    public string Name { get; }
    public EWorkflowState WorkflowState { get; }
    public ProcessFlow? ProcessFlow { get; }
}