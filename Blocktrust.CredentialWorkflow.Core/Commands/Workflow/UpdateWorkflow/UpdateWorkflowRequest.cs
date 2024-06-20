namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.UpdateWorkflow;

using FluentResults;
using MediatR;
using Domain.Enums;
using Domain.Workflow;

public class UpdateWorkflowRequest : IRequest<Result<Workflow>>
{
    public UpdateWorkflowRequest(Guid workflowId, string name, EWorkflowState workflowState, string? configurationJson)
    {
        WorkflowId = workflowId;
        Name = name;
        WorkflowState = workflowState;
        ConfigurationJson = configurationJson;
    }

    public Guid WorkflowId { get; }
    public string Name { get; }
    public EWorkflowState WorkflowState { get; }
    public string? ConfigurationJson { get; }
}