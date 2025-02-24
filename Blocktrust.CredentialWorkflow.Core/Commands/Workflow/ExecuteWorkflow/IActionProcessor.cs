using Blocktrust.CredentialWorkflow.Core.Domain.Enums;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;
using FluentResults;
using System.Threading.Tasks;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow;

using Action = Domain.ProcessFlow.Actions.Action;

public interface IActionProcessor
{
    EActionType ActionType { get; }
    Task<Result> ProcessAsync(Action action, ActionOutcome actionOutcome, ActionProcessingContext context);
}