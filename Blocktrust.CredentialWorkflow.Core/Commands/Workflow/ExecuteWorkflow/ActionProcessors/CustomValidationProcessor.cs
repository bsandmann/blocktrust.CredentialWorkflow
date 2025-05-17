using System.Text.Json;
using Blocktrust.CredentialWorkflow.Core.Commands.ValidateCredentials.CustomValidation;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Validation;
using FluentResults;
using MediatR;
using Action = Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Action;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow.ActionProcessors;

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

        object data;
        try
        {
            // First, try to deserialize as JSON object
            data = JsonSerializer.Deserialize<object>(dataStr);
            if (data == null)
            {
                var errorMessage = "No data found in the execution context to validate.";
                actionOutcome.FinishOutcomeWithFailure(errorMessage);
                return Result.Fail(errorMessage);
            }
        }
        catch (JsonException)
        {
            // If JSON deserialization fails, handle as a simple string value
            // Wrap the string in a JSON object with a "value" property
            try
            {
                // Create a simple wrapper around the string value
                // Remove any special characters that could interfere with JSON serialization
                string sanitizedStr = dataStr.Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n");
                string wrappedJson = $"{{\"value\": \"{sanitizedStr}\"}}";
                data = JsonSerializer.Deserialize<object>(wrappedJson);
                
                if (data == null)
                {
                    var errorMessage = "Failed to process data for validation.";
                    actionOutcome.FinishOutcomeWithFailure(errorMessage);
                    return Result.Fail(errorMessage);
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"Failed to process data for validation: {ex.Message}";
                actionOutcome.FinishOutcomeWithFailure(errorMessage);
                return Result.Fail(errorMessage);
            }
        }

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