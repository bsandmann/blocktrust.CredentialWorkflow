namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow.ActionProcessors;

using System.Text.Json;
using System.Threading.Tasks;
using Blocktrust.CredentialWorkflow.Core.Commands.ValidateCredentials.W3cValidation;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Validation;
using FluentResults;
using MediatR;

public class W3cValidationProcessor : IActionProcessor
{
    private readonly IMediator _mediator;

    public W3cValidationProcessor(IMediator mediator)
    {
        _mediator = mediator;
    }

    public EActionType ActionType => EActionType.W3cValidation;

    public async Task<Result> ProcessAsync(Action action, ActionOutcome actionOutcome, ActionProcessingContext context)
    {
        var input = (W3cValidationAction)action.Input;

        var credentialStr = await ParameterResolver.GetParameterFromExecutionContext(
            input.CredentialReference, context.ExecutionContext, context.Workflow, context.ActionOutcomes, ActionType, _mediator);
        if (string.IsNullOrWhiteSpace(credentialStr))
        {
            var errorMessage = "No credential found in the execution context to validate.";
            actionOutcome.FinishOutcomeWithFailure(errorMessage);
            return Result.Fail(errorMessage);
        }

        var validationRequest = new W3cValidationRequest(credentialStr, input.ValidationRules);
        var validationResult = await _mediator.Send(validationRequest, context.CancellationToken);
        if (validationResult.IsFailed)
        {
            var errorMessage = string.Join(", ", validationResult.Errors.Select(e => e.Message));
            actionOutcome.FinishOutcomeWithFailure(errorMessage);
            return Result.Fail(errorMessage);
        }

        var result = validationResult.Value;
        if (!result.IsValid)
        {
            var errorMessage = string.Join(", ", result.Errors.Select(e => $"{e.RuleType}: {e.Message}"));
            actionOutcome.FinishOutcomeWithFailure(errorMessage);
            return Result.Fail(errorMessage);
        }

        actionOutcome.FinishOutcomeWithSuccess(JsonSerializer.Serialize(result));
        return Result.Ok();
    }
}