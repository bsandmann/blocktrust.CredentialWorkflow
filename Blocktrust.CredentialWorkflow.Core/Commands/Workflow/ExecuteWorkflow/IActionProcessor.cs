using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;
using FluentResults;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow;

using Action = Domain.ProcessFlow.Actions.Action;

public interface IActionProcessor
{
    EActionType ActionType { get; }
    Task<FluentResults.Result> ProcessAsync(Action action, ActionOutcome actionOutcome, ActionProcessingContext context);
}