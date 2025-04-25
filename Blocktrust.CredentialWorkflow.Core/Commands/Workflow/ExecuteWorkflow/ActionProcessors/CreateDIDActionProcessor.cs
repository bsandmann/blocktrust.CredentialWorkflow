using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.GetTenantInformation;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.DID;
using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow.ActionProcessors;

using System.Diagnostics;
using System.Text.Json;
using Services;
using Action = Domain.ProcessFlow.Actions.Action;

public class CreateDIDActionProcessor : IActionProcessor
{
    private readonly IMediator _mediator;
    private readonly IHttpClientFactory _httpClientFactory;

    public CreateDIDActionProcessor(IMediator mediator, IHttpClientFactory httpClientFactory)
    {
        _mediator = mediator;
        _httpClientFactory = httpClientFactory;
    }


    public EActionType ActionType => EActionType.CreateDID;
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan TimeoutLimit = TimeSpan.FromMinutes(10);

    public async Task<FluentResults.Result> ProcessAsync(Action action, ActionOutcome actionOutcome, ActionProcessingContext context)
    {
        var input = (CreateDIDAction)action.Input;

        try
        {
            // Get registration parameters
            var registrarUrl = string.Empty;
            var walletId = string.Empty;

            if (input.UseTenantRegistrar)
            {
                // Use tenant registrar URL from context
                var tenantId = context.Workflow.TenantId;
                var tenantInfoRequest = new GetTenantInformationRequest(tenantId);
                var tenantInfoResult = await _mediator.Send(tenantInfoRequest, context.CancellationToken);

                if (tenantInfoResult.IsFailed)
                {
                    var errorMessage = $"Failed to get tenant information: {tenantInfoResult.Errors.FirstOrDefault()?.Message}";
                    actionOutcome.FinishOutcomeWithFailure(errorMessage);
                    return Result.Fail(errorMessage);
                }

                registrarUrl = tenantInfoResult.Value.Tenant.OpnRegistrarUrl;
                if (string.IsNullOrWhiteSpace(registrarUrl))
                {
                    var errorMessage = "The tenant does not have a registrar URL configured.";
                    actionOutcome.FinishOutcomeWithFailure(errorMessage);
                    return Result.Fail(errorMessage);
                }

                walletId = tenantInfoResult.Value.Tenant.WalletId;
                if (string.IsNullOrWhiteSpace(walletId))
                {
                    var errorMessage = "The tenant does not have a wallet ID configured.";
                    actionOutcome.FinishOutcomeWithFailure(errorMessage);
                    return Result.Fail(errorMessage);
                }
            }
            else
            {
                // Get custom registrar URL
                registrarUrl = await ParameterResolver.GetParameterFromExecutionContext(
                    input.RegistrarUrl, context.ExecutionContext, context.Workflow, context.ActionOutcomes, ActionType, _mediator);

                if (string.IsNullOrWhiteSpace(registrarUrl))
                {
                    var errorMessage = "No registrar URL provided for DID creation.";
                    actionOutcome.FinishOutcomeWithFailure(errorMessage);
                    return Result.Fail(errorMessage);
                }

                // Get custom wallet ID
                walletId = await ParameterResolver.GetParameterFromExecutionContext(
                    input.WalletId, context.ExecutionContext, context.Workflow, context.ActionOutcomes, ActionType, _mediator);

                if (string.IsNullOrWhiteSpace(walletId))
                {
                    var errorMessage = "No wallet ID provided for DID creation.";
                    actionOutcome.FinishOutcomeWithFailure(errorMessage);
                    return Result.Fail(errorMessage);
                }
            }

            // Process verification methods
            if (input.VerificationMethods == null || !input.VerificationMethods.Any())
            {
                var errorMessage = "At least one verification method is required for DID creation.";
                actionOutcome.FinishOutcomeWithFailure(errorMessage);
                return Result.Fail(errorMessage);
            }

            // Process verification methods
            var verificationMethods = new List<Dictionary<string, string>>();
            foreach (var method in input.VerificationMethods)
            {
                var keyId = await ParameterResolver.GetParameterFromExecutionContext(
                    method.KeyId, context.ExecutionContext, context.Workflow, context.ActionOutcomes, ActionType, _mediator);

                var purpose = await ParameterResolver.GetParameterFromExecutionContext(
                    method.Purpose, context.ExecutionContext, context.Workflow, context.ActionOutcomes, ActionType, _mediator);

                var curve = await ParameterResolver.GetParameterFromExecutionContext(
                    method.Curve, context.ExecutionContext, context.Workflow, context.ActionOutcomes, ActionType, _mediator);

                if (string.IsNullOrWhiteSpace(keyId) || string.IsNullOrWhiteSpace(purpose) || string.IsNullOrWhiteSpace(curve))
                {
                    var errorMessage = "Invalid verification method parameters.";
                    actionOutcome.FinishOutcomeWithFailure(errorMessage);
                    return Result.Fail(errorMessage);
                }

                verificationMethods.Add(new Dictionary<string, string>
                {
                    { "keyId", keyId.ToLowerInvariant() },
                    { "purpose", purpose },
                    { "curve", curve }
                });
            }

            // Process services if any
            var services = new List<Dictionary<string, string>>();
            if (input.Services != null && input.Services.Any())
            {
                foreach (var service in input.Services)
                {
                    var serviceId = await ParameterResolver.GetParameterFromExecutionContext(
                        service.ServiceId, context.ExecutionContext, context.Workflow, context.ActionOutcomes, ActionType, _mediator);

                    var type = await ParameterResolver.GetParameterFromExecutionContext(
                        service.Type, context.ExecutionContext, context.Workflow, context.ActionOutcomes, ActionType, _mediator);

                    var endpoint = await ParameterResolver.GetParameterFromExecutionContext(
                        service.Endpoint, context.ExecutionContext, context.Workflow, context.ActionOutcomes, ActionType, _mediator);

                    if (string.IsNullOrWhiteSpace(serviceId) || string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(endpoint))
                    {
                        var errorMessage = "Invalid service endpoint parameters.";
                        actionOutcome.FinishOutcomeWithFailure(errorMessage);
                        return Result.Fail(errorMessage);
                    }

                    services.Add(new Dictionary<string, string>
                    {
                        { "serviceId", serviceId.ToLowerInvariant() },
                        { "type", type },
                        { "endpoint", endpoint }
                    });
                }
            }

            var client = new OpenPrismNodeRegistrarClient(_httpClientFactory);
            var result = await client.CreateDidAsync(registrarUrl, walletId, verificationMethods, services);
            if (result.IsFailed)
            {
                var errorMessage = $"Failed to create DID: {result.Errors.FirstOrDefault()?.Message}";
                actionOutcome.FinishOutcomeWithFailure(errorMessage);
                return Result.Fail(errorMessage);
            }

            if (result.Value.DidState.State == "failed")
            {
                var errorMessage = $"Failed to create DID on the OPN Registrar: {result.Value.DidState.Reason}";
                actionOutcome.FinishOutcomeWithFailure(errorMessage);
                return Result.Fail(errorMessage);
            }

            var jobId = result.Value.JobId;
            if (string.IsNullOrEmpty(jobId))
            {
                var errorMessage = $"Failed to create DID: OPN Registrar returned an invalid job ID";
                actionOutcome.FinishOutcomeWithFailure(errorMessage);
                return Result.Fail(errorMessage);
            }

            // --- Start of Polling Implementation ---
            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.Elapsed < TimeoutLimit)
            {
                // Check for cancellation request
                context.CancellationToken.ThrowIfCancellationRequested();

                var jobStatus = await client.GetCreateResultAsync(registrarUrl, jobId);

                if (jobStatus.IsFailed)
                {
                    // Log the error but continue polling unless it's a permanent failure?
                    // For now, let's treat a failed status check as a reason to stop and fail the action.
                    var errorMessage = $"Failed to get job status: {jobStatus.Errors.FirstOrDefault()?.Message}";
                    actionOutcome.FinishOutcomeWithFailure(errorMessage);
                    stopwatch.Stop();
                    return Result.Fail(errorMessage);
                }

                var currentState = jobStatus.Value.DidState?.State;

                if (currentState == "finished")
                {
                    var resultString = JsonSerializer.Serialize(jobStatus.Value.DidState, new JsonSerializerOptions() { IgnoreNullValues = true });
                    actionOutcome.FinishOutcomeWithSuccess(resultString);
                    stopwatch.Stop();
                    return Result.Ok();
                }

                if (currentState == "failed")
                {
                    var errorMessage = $"Failed creation operation: {jobStatus.Value.DidState?.Reason ?? "No reason provided."}";
                    actionOutcome.FinishOutcomeWithFailure(errorMessage);
                    stopwatch.Stop();
                    return Result.Fail(errorMessage);
                }

                // If state is "wait" or potentially null/unknown, continue waiting
                if (currentState == "wait")
                {
                    // Explicitly doing nothing, just wait for the delay
                }
                else if (string.IsNullOrEmpty(currentState))
                {
                }
                // else: Handle any other unexpected states if necessary, potentially logging them.


                // Wait before the next poll, respecting cancellation token
                try
                {
                    await Task.Delay(PollingInterval, context.CancellationToken);
                }
                catch (TaskCanceledException)
                {
                    // Operation was cancelled during the delay
                    actionOutcome.FinishOutcomeWithFailure("Operation cancelled during status check.");
                    stopwatch.Stop();
                    throw; // Re-throw the cancellation exception
                }
            }

            // If the loop finishes without returning, it means timeout
            stopwatch.Stop();
            var timeoutErrorMessage = $"DID creation status check timed out after {TimeoutLimit.TotalMinutes} minutes for job ID {jobId}.";
            actionOutcome.FinishOutcomeWithFailure(timeoutErrorMessage);
            return Result.Fail(timeoutErrorMessage);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error processing CreateDID action: {ex.Message}";
            actionOutcome.FinishOutcomeWithFailure(errorMessage);
            return Result.Fail(errorMessage);
        }
    }
}