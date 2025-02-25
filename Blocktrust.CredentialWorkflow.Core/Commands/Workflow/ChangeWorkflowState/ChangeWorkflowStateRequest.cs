namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ChangeWorkflowState;

using System;
using FluentResults;
using MediatR;
using Domain.Enums;
using Domain.Workflow;

public class ChangeWorkflowStateRequest : IRequest<Result<Workflow>>
{
    public ChangeWorkflowStateRequest(Guid workflowId, EWorkflowState workflowState)
    {
        WorkflowId = workflowId;
        WorkflowState = workflowState;
    }

    public Guid WorkflowId { get; }
    public EWorkflowState WorkflowState { get; }
}