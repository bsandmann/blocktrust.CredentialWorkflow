using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.JWT;
using FluentResults;
using MediatR;
using System.Text.Json;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow.ActionProcessors;

using Action = Domain.ProcessFlow.Actions.Action;

public class JwtTokenGeneratorActionProcessor : IActionProcessor
{
    private readonly IMediator _mediator;
    private readonly IHttpClientFactory _httpClientFactory;

    public JwtTokenGeneratorActionProcessor(IMediator mediator, IHttpClientFactory httpClientFactory)
    {
        _mediator = mediator;
        _httpClientFactory = httpClientFactory;
    }

    public EActionType ActionType => EActionType.JwtTokenGenerator;

    public async Task<Result> ProcessAsync(Action action, ActionOutcome actionOutcome, ActionProcessingContext context)
    {
        var input = (JwtTokenGeneratorAction)action.Input;

        try
        {
            // Get issuer
            var issuer = await ParameterResolver.GetParameterFromExecutionContext(
                input.Issuer, context.ExecutionContext, context.Workflow, context.ActionOutcomes, ActionType, _mediator);

            if (string.IsNullOrWhiteSpace(issuer))
            {
                var errorMessage = "No issuer provided for JWT token generation.";
                actionOutcome.FinishOutcomeWithFailure(errorMessage);
                return Result.Fail(errorMessage);
            }

            // Get audience
            var audience = await ParameterResolver.GetParameterFromExecutionContext(
                input.Audience, context.ExecutionContext, context.Workflow, context.ActionOutcomes, ActionType, _mediator);

            if (string.IsNullOrWhiteSpace(audience))
            {
                var errorMessage = "No audience provided for JWT token generation.";
                actionOutcome.FinishOutcomeWithFailure(errorMessage);
                return Result.Fail(errorMessage);
            }

            // Get subject
            var subject = await ParameterResolver.GetParameterFromExecutionContext(
                input.Subject, context.ExecutionContext, context.Workflow, context.ActionOutcomes, ActionType, _mediator);

            // Parse expiration
            var expirationStr = await ParameterResolver.GetParameterFromExecutionContext(
                input.Expiration, context.ExecutionContext, context.Workflow, context.ActionOutcomes, ActionType, _mediator);

            if (string.IsNullOrWhiteSpace(expirationStr) || !int.TryParse(expirationStr, out int expirationSeconds))
            {
                var errorMessage = "Invalid expiration provided for JWT token generation.";
                actionOutcome.FinishOutcomeWithFailure(errorMessage);
                return Result.Fail(errorMessage);
            }

            // Get claims
            var claims = new Dictionary<string, string>();

            if (input.ClaimsFromPreviousAction && input.PreviousActionId.HasValue)
            {
                // TODO: Get claims from previous action
                // This is where you would extract claims from the previous action's outcome
                // For now, this is left as a placeholder to be implemented manually
            }
            else
            {
                //Process manually defined claims
                foreach (var claimPair in input.Claims)
                {
                    var key = claimPair.Key;
                    var claimValue = claimPair.Value;

                    string resolvedValue;
                    if (claimValue.Type == ClaimValueType.Static)
                    {
                        resolvedValue = claimValue.Value;
                    }
                    else if (claimValue.Type == ClaimValueType.TriggerProperty && claimValue.ParameterReference != null)
                    {
                        resolvedValue = await ParameterResolver.GetParameterFromExecutionContext(
                            claimValue.ParameterReference, context.ExecutionContext, context.Workflow, context.ActionOutcomes, ActionType, _mediator);
                    }
                    else
                    {
                        continue; // Skip invalid claim
                    }

                    if (!string.IsNullOrEmpty(resolvedValue))
                    {
                        claims[key] = resolvedValue;
                    }
                }
            }

            // TODO: Generate the actual JWT token
            // This is where the actual JWT token generation logic would be implemented
            // For now, we'll just return a success with a placeholder payload

            var jwtPayload = new
            {
                iss = issuer,
                sub = subject,
                aud = audience,
                exp = DateTimeOffset.UtcNow.AddSeconds(expirationSeconds).ToUnixTimeSeconds(),
                iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                claims
            };

            var resultJson = JsonSerializer.Serialize(jwtPayload);
            actionOutcome.FinishOutcomeWithSuccess(resultJson);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error processing JWT token generation: {ex.Message}";
            actionOutcome.FinishOutcomeWithFailure(errorMessage);
            return Result.Fail(errorMessage);
        }
    }
}