using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.JWT;
using FluentResults;
using MediatR;
using System.Text.Json;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow.ActionProcessors;

using Services;
using VerifiableCredential;
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
                var outcomeJson = context.ActionOutcomes.FirstOrDefault()?.OutcomeJson;
                if (string.IsNullOrWhiteSpace(outcomeJson))
                {
                    var errorMessage = "Cannot find valid previous action outcome.";
                    actionOutcome.FinishOutcomeWithFailure(errorMessage);
                    return Result.Fail(errorMessage);
                }

                if (outcomeJson.StartsWith("ey"))
                {
                    var credentialResult = JwtParser.Parse(outcomeJson);
                    if (credentialResult.IsFailed)
                    {
                        var errorMessage = $"Could not parse credential from previous action outcome: {credentialResult.Errors.FirstOrDefault()?.Message}";
                        actionOutcome.FinishOutcomeWithFailure(errorMessage);
                        return Result.Fail(errorMessage);
                    }

                    var credential = credentialResult.Value;
                    if (credential.VerifiableCredentials.FirstOrDefault() != null)
                    {
                        var subjects = credential.VerifiableCredentials.FirstOrDefault().CredentialSubjects;
                        if (subjects != null)
                        {
                            foreach (var claim in subjects.FirstOrDefault().AdditionalData)
                            {
                                var key = claim.Key;
                                var claimValue = claim.Value;

                                if (claimValue is string stringValue)
                                {
                                    claims[key] = stringValue;
                                }
                                else if (claimValue is JsonElement jsonElement)
                                {
                                    claims[key] = jsonElement.ToString();
                                }
                            }
                        }
                    }
                }
                else
                {
                    var errorMessage = "Unexpected outcome format from previous action.";
                    actionOutcome.FinishOutcomeWithFailure(errorMessage);
                    return Result.Fail(errorMessage);
                }
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