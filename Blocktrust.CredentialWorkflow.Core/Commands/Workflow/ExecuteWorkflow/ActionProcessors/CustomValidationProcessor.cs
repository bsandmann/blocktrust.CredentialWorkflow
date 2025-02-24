namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow.ActionProcessors;

using System.Text.Json;
using System.Threading.Tasks;
using Blocktrust.CredentialWorkflow.Core.Commands.ValidateCredentials.CustomValidation;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Validation;
using FluentResults;
using MediatR;

public class CustomValidationProcessor : IActionProcessor
{
    private readonly IMediator _mediator;

    public CustomValidationProcessor(IMediator mediator)
    {
        _mediator = mediator;
    }

    public EActionType ActionType => EActionType.CustomValidation;

    public async Task<Result> ProcessAsync(Action action, ActionOutcome actionOutcome, ActionProcessingContext context)
    {
        var input = (CustomValidationAction)action.Input;

        var dataStr = await ParameterResolver.GetParameterFromExecutionContext(
            input.DataReference, context.ExecutionContext, context.Workflow, context.ActionOutcomes, ActionType, _mediator);
        if (string.IsNullOrWhiteSpace(dataStr))
        {
            var errorMessage = "No data found in the execution context to validate.";
            actionOutcome.FinishOutcomeWithFailure(errorMessage);
            return Result.Fail(errorMessage);
        }

        var data = JsonSerializer.Deserialize<object>(dataStr);
        var validationRequest = new CustomValidationRequest(data, input.ValidationRules);
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
            var errorMessage = string.Join(", ", result.Errors.Select(e => $"{e.RuleName}: {e.Message}"));
            actionOutcome.FinishOutcomeWithFailure(errorMessage);
            return Result.Fail(errorMessage);
        }

        actionOutcome.FinishOutcomeWithSuccess(JsonSerializer.Serialize(result));
        return Result.Ok();
    }
}