using System.Text.Json;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow.ActionProcessors;
using Blocktrust.CredentialWorkflow.Core.Commands.WorkflowOutcome.UpdateWorkflowOutcome;
using Blocktrust.CredentialWorkflow.Core.Domain.Enums;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Triggers;
using FluentResults;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow;

using GetWorkflowById;
using Action = Domain.ProcessFlow.Actions.Action;
using ExecutionContext = Domain.Common.ExecutionContext;

public class ExecuteWorkflowHandler : IRequestHandler<ExecuteWorkflowRequest, Result<bool>>
{
    private readonly IMediator _mediator;
    private readonly IServiceProvider _serviceProvider;

    public ExecuteWorkflowHandler(IMediator mediator, IServiceProvider serviceProvider)
    {
        _mediator = mediator;
        _serviceProvider = serviceProvider;
    }

    public async Task<Result<bool>> Handle(ExecuteWorkflowRequest request, CancellationToken cancellationToken)
    {
        var workflowId = request.WorkflowOutcome.WorkflowId;
        var workflowOutcomeId = request.WorkflowOutcome.WorkflowOutcomeId;
        var executionContextString = request.WorkflowOutcome.ExecutionContext;

        var workflowResult = await _mediator.Send(new GetWorkflowByIdRequest(workflowId), cancellationToken);
        if (workflowResult.IsFailed)
        {
            return Result.Fail("Unable to load Workflow");
        }

        var workflow = workflowResult.Value;

        // Build up execution context
        ExecutionContext executionContext = BuildExecutionContext(workflow, executionContextString);

        if (workflow.ProcessFlow is null || workflow.ProcessFlow.Triggers.Count != 1)
        {
            return Result.Fail("Unable to process Workflow. No process flow definition found.");
        }

        var actionOutcomes = new List<ActionOutcome>();
        var triggerId = workflow.ProcessFlow.Triggers.Single().Key;

        // For each loop iteration, we figure out which action to run next.
        // Start with no "previous action". The first action references the triggerId in the runAfter.
        Guid? previousActionId = null;

        while (true)
        {
            var nextActionKvp = FindNextAction(
                workflow.ProcessFlow.Actions,
                triggerId,
                previousActionId
            );

            if (nextActionKvp is null)
            {
                break;
            }

            var actionId = nextActionKvp.Value.Key;
            var action = nextActionKvp.Value.Value;

            var actionOutcome = new ActionOutcome(actionId);

            // Resolve the appropriate IActionProcessor for the action type
            var processor = ResolveActionProcessor(action.Type);
            if (processor == null)
            {
                return Result.Fail($"No processor found for action type {action.Type}.");
            }

            // Create the processing context
            var processingContext = new ActionProcessingContext(
                executionContext,
                actionOutcomes,
                workflow,
                cancellationToken
            );

            // Process the action
            var processResult = await processor.ProcessAsync(action, actionOutcome, processingContext);
            if (processResult.IsFailed)
            {
                return await FinishActionsWithFailure(workflowOutcomeId, actionOutcome, processResult.Errors.First().Message, actionOutcomes, cancellationToken);
            }

            actionOutcomes.Add(actionOutcome);
            previousActionId = actionId;
        }

        return await FinishActionsWithSuccess(workflowOutcomeId, actionOutcomes, cancellationToken);
    }

    private IActionProcessor? ResolveActionProcessor(EActionType actionType)
    {
        return actionType switch
        {
            EActionType.IssueW3CCredential => _serviceProvider.GetService<IssueW3CCredentialProcessor>(),
            EActionType.VerifyW3CCredential => _serviceProvider.GetService<VerifyW3CCredentialProcessor>(),
            EActionType.Email => _serviceProvider.GetService<EmailActionProcessor>(),
            EActionType.DIDComm => _serviceProvider.GetService<DIDCommActionProcessor>(),
            EActionType.W3cValidation => _serviceProvider.GetService<W3cValidationProcessor>(),
            EActionType.CustomValidation => _serviceProvider.GetService<CustomValidationProcessor>(),
            EActionType.CreateDID => _serviceProvider.GetService<CreateDIDActionProcessor>(),
            EActionType.UpdateDID => _serviceProvider.GetService<UpdateDIDActionProcessor>(),
            EActionType.DeleteDID => _serviceProvider.GetService<DeactivateDIDActionProcessor>(),
            EActionType.JwtTokenGenerator => _serviceProvider.GetService<JwtTokenGeneratorActionProcessor>(),
            _ => null
        };
    }

    private KeyValuePair<Guid, Action>? FindNextAction(
        Dictionary<Guid, Action> actions,
        Guid triggerId,
        Guid? previousActionId
    )
    {
        var predecessorId = previousActionId ?? triggerId;

        var nextAction = actions
            .SingleOrDefault(x => x.Value.RunAfter.Count == 1
                                  && x.Value.RunAfter.Single() == predecessorId);

        if (nextAction.Key == default && nextAction.Value == null)
        {
            return null;
        }

        return nextAction;
    }

    private ExecutionContext BuildExecutionContext(Domain.Workflow.Workflow workflow, string? executionContextString)
    {
        var trigger = workflow?.ProcessFlow?.Triggers.FirstOrDefault().Value;
        if (trigger is null)
        {
            return new ExecutionContext(workflow!.TenantId);
        }

        if (trigger.Type == ETriggerType.HttpRequest && executionContextString is not null)
        {
            return ExecutionContext.FromSimplifiedHttpContext(workflow.TenantId, executionContextString);
        }
        if (trigger.Type == ETriggerType.Form && executionContextString is not null)
        {
            return ExecutionContext.FromForm(workflow.TenantId, executionContextString);
        }
        if (trigger.Type == ETriggerType.WalletInteraction && executionContextString is not null)
        {
            return ExecutionContext.FromSimplifiedHttpContext(workflow.TenantId, executionContextString);
        }

        return new ExecutionContext(workflow!.TenantId);
    }

    private async Task<Result<bool>> FinishActionsWithSuccess(
        Guid workflowOutcomeId,
        List<ActionOutcome> actionOutcomes,
        CancellationToken cancellationToken)
    {
        var workflowUpdateResult = await _mediator.Send(
            new UpdateWorkflowOutcomeRequest(
                workflowOutcomeId,
                EWorkflowOutcomeState.Success,
                JsonSerializer.Serialize(actionOutcomes),
                null),
            cancellationToken);

        if (workflowUpdateResult.IsFailed)
        {
            return Result.Fail("The workflow outcome could not be updated.");
        }

        return Result.Ok(true);
    }

    private async Task<Result<bool>> FinishActionsWithFailure(
        Guid workflowOutcomeId,
        ActionOutcome actionOutcome,
        string errorMessage,
        List<ActionOutcome> actionOutcomes,
        CancellationToken cancellationToken
    )
    {
        actionOutcome.FinishOutcomeWithFailure(errorMessage);
        actionOutcomes.Add(actionOutcome);

        var workflowUpdateResult = await _mediator.Send(
            new UpdateWorkflowOutcomeRequest(
                workflowOutcomeId,
                EWorkflowOutcomeState.FailedWithErrors,
                JsonSerializer.Serialize(actionOutcomes),
                errorMessage),
            cancellationToken);

        if (workflowUpdateResult.IsFailed)
        {
            return Result.Fail("The workflow outcome could not be updated.");
        }

        return Result.Ok(false);
    }
}