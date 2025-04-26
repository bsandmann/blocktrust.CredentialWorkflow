using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.GetTenantInformation;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.DID;
using Blocktrust.CredentialWorkflow.Core.Domain.Common;
using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow.ActionProcessors;

using System.Diagnostics;
using System.Text.Json;
using Prism;
using Services;
using Action = Domain.ProcessFlow.Actions.Action;

public class DeactivateDIDActionProcessor : IActionProcessor
{
    private readonly IMediator _mediator;
    private readonly IHttpClientFactory _httpClientFactory;

    public DeactivateDIDActionProcessor(IMediator mediator, IHttpClientFactory httpClientFactory)
    {
        _mediator = mediator;
        _httpClientFactory = httpClientFactory;
    }

    public EActionType ActionType => EActionType.DeleteDID;
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan TimeoutLimit = TimeSpan.FromMinutes(10);

    public async Task<FluentResults.Result> ProcessAsync(Action action, ActionOutcome actionOutcome, ActionProcessingContext context)
    {
        // Input can be either DeactivateDIDAction or DeleteDIDAction (which inherits from DeactivateDIDAction)
        DeactivateDIDAction? input = action.Input as DeactivateDIDAction;

        if (input == null)
        {
            var errorMessage = "Invalid action input type for DeactivateDIDActionProcessor";
            actionOutcome.FinishOutcomeWithFailure(errorMessage);
            return Result.Fail(errorMessage);
        }

        try
        {
            // Get registration parameters
            var registrarUrl = string.Empty;
            var walletId = string.Empty;
            var did = string.Empty;

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
                    var errorMessage = "No registrar URL provided for DID deactivation.";
                    actionOutcome.FinishOutcomeWithFailure(errorMessage);
                    return Result.Fail(errorMessage);
                }

                // Get custom wallet ID
                walletId = await ParameterResolver.GetParameterFromExecutionContext(
                    input.WalletId, context.ExecutionContext, context.Workflow, context.ActionOutcomes, ActionType, _mediator);

                if (string.IsNullOrWhiteSpace(walletId))
                {
                    var errorMessage = "No wallet ID provided for DID deactivation.";
                    actionOutcome.FinishOutcomeWithFailure(errorMessage);
                    return Result.Fail(errorMessage);
                }
            }

            // Get DID to deactivate
            did = await ParameterResolver.GetParameterFromExecutionContext(
                input.Did, context.ExecutionContext, context.Workflow, context.ActionOutcomes, ActionType, _mediator);

            if (string.IsNullOrWhiteSpace(did))
            {
                var errorMessage = "No DID provided for deactivation operation.";
                actionOutcome.FinishOutcomeWithFailure(errorMessage);
                return Result.Fail(errorMessage);
            }

            // Check if the input might be JSON
            if (did.TrimStart().StartsWith("{")) // TrimStart to handle potential leading whitespace
            {
                // We likely have JSON, try to parse and extract the "did" property
                try
                {
                    using (JsonDocument document = JsonDocument.Parse(did))
                    {
                        if (document.RootElement.TryGetProperty("did", out JsonElement didElement) && didElement.ValueKind == JsonValueKind.String)
                        {
                            string extractedDid = didElement.GetString();
                            if (!string.IsNullOrWhiteSpace(extractedDid))
                            {
                                // Successfully extracted the DID, update the variable
                                did = extractedDid;
                            }
                            else
                            {
                                // Found "did" key but its value is null or empty/whitespace
                                var errorMessage = "Found 'did' property in JSON, but its value is null or empty.";
                                actionOutcome.FinishOutcomeWithFailure(errorMessage);
                                return Result.Fail(errorMessage);
                            }
                        }
                        else
                        {
                            // JSON does not contain a string property named "did" at the root level
                            var errorMessage = "Input appears to be JSON, but could not find a valid 'did' string property.";
                            actionOutcome.FinishOutcomeWithFailure(errorMessage);
                            return Result.Fail(errorMessage);
                        }
                    }
                }
                catch (JsonException jsonEx)
                {
                    // The string started with '{' but wasn't valid JSON
                    var errorMessage = $"Input started with '{{' but failed to parse as JSON: {jsonEx.Message}";
                    actionOutcome.FinishOutcomeWithFailure(errorMessage);
                    return Result.Fail(errorMessage);
                    // Alternatively, you might decide to treat it as a regular string if parsing fails,
                    // depending on your exact requirements, but failing seems safer here.
                }
            }

            if (!did.StartsWith("did:prism:"))
            {
                var errorMessage = "DID must be in format 'did:prism:...'";
                actionOutcome.FinishOutcomeWithFailure(errorMessage);
                return Result.Fail(errorMessage);
            }

            // Optional: Get Master Key Secret if provided
            string? masterKeySecret = null;
            if (input.MasterKeySecret != null)
            {
                masterKeySecret = await ParameterResolver.GetParameterFromExecutionContext(
                    input.MasterKeySecret, context.ExecutionContext, context.Workflow, context.ActionOutcomes, ActionType, _mediator);

                // Check if the input might be JSON and is not null/empty
                if (!string.IsNullOrWhiteSpace(masterKeySecret) && masterKeySecret.TrimStart().StartsWith("{"))
                {
                    try
                    {
                        using (JsonDocument document = JsonDocument.Parse(masterKeySecret))
                        {
                            // Navigate to the "verificationMethod" array within "secret"
                            if (document.RootElement.TryGetProperty("secret", out JsonElement secretElement) && secretElement.ValueKind == JsonValueKind.Object &&
                                secretElement.TryGetProperty("verificationMethod", out JsonElement verificationMethods) && verificationMethods.ValueKind == JsonValueKind.Array)
                            {
                                // Find the first verification method object where the "purpose" array contains "masterKey"
                                var masterKeyVm = verificationMethods.EnumerateArray()
                                    .FirstOrDefault(vm => // Find the first element that matches the criteria
                                        vm.TryGetProperty("purpose", out JsonElement purposes) && // Check if "purpose" property exists
                                        purposes.ValueKind == JsonValueKind.Array && // Check if "purpose" is an array
                                        purposes.EnumerateArray().Any(p => // Check if any element within the "purpose" array...
                                            p.ValueKind == JsonValueKind.String && // ...is a string...
                                            p.GetString() == "masterKey")); // ...and equals "masterKey"

                                // Check if we found the target verification method (FirstOrDefault returns default(JsonElement) if not found)
                                if (masterKeyVm.ValueKind != JsonValueKind.Undefined)
                                {
                                    // Now extract the "d" value from its "privateKeyJwk"
                                    if (masterKeyVm.TryGetProperty("privateKeyJwk", out JsonElement privateKeyJwk) && privateKeyJwk.ValueKind == JsonValueKind.Object &&
                                        privateKeyJwk.TryGetProperty("d", out JsonElement dElement) && dElement.ValueKind == JsonValueKind.String)
                                    {
                                        string extractedDValue = dElement.GetString();
                                        if (!string.IsNullOrWhiteSpace(extractedDValue))
                                        {
                                            // Successfully extracted the 'd' value, update the variable
                                            masterKeySecret = extractedDValue;
                                        }
                                        else
                                        {
                                            // Found the structure, but 'd' value is null or empty - violates guarantee? Handle defensively.
                                            var errorMessage = "Found 'masterKey' verification method and 'privateKeyJwk.d', but its value is null or empty.";
                                            actionOutcome.FinishOutcomeWithFailure(errorMessage); // Assuming actionOutcome is available
                                            return Result.Fail(errorMessage); // Assuming Result is available
                                        }
                                    }
                                    else
                                    {
                                        // Found the masterKey VM, but couldn't find "privateKeyJwk" or "d" string within it - violates guarantee.
                                        var errorMessage = "Found 'masterKey' verification method, but failed to find 'privateKeyJwk.d' string property within it.";
                                        actionOutcome.FinishOutcomeWithFailure(errorMessage);
                                        return Result.Fail(errorMessage);
                                    }
                                }
                                else
                                {
                                    // The guarantee was violated - no VM with purpose "masterKey" found.
                                    var errorMessage = "JSON parsed, but failed to find a verification method with 'masterKey' purpose.";
                                    actionOutcome.FinishOutcomeWithFailure(errorMessage);
                                    return Result.Fail(errorMessage);
                                }
                            }
                            else
                            {
                                // JSON structure is missing "secret" or "secret.verificationMethod" array.
                                var errorMessage = "JSON input is missing the expected 'secret.verificationMethod' array structure.";
                                actionOutcome.FinishOutcomeWithFailure(errorMessage);
                                return Result.Fail(errorMessage);
                            }
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        // Handle JSON parsing errors
                        var errorMessage = $"Input started with '{{' but failed to parse as JSON: {jsonEx.Message}";
                        actionOutcome.FinishOutcomeWithFailure(errorMessage);
                        return Result.Fail(errorMessage);
                    }
                    // If parsing and extraction were successful, masterKeySecret now holds the 'd' value.
                    // If it wasn't JSON or parsing failed, masterKeySecret retains its original value (which might be null/empty or non-JSON).
                }

                // Validate Base64 format if provided
                if (!string.IsNullOrWhiteSpace(masterKeySecret) && !PrismEncoding.IsValidBase64(masterKeySecret))
                {
                    var errorMessage = "Master Key Secret must be a valid base64 encoded string.";
                    actionOutcome.FinishOutcomeWithFailure(errorMessage);
                    return Result.Fail(errorMessage);
                }
            }

            var client = new OpenPrismNodeRegistrarClient(_httpClientFactory);
            var result = await client.DeactivateDidAsync(
                registrarUrl,
                did,
                walletId,
                masterKeySecret,
                context.CancellationToken);

            if (result.IsFailed)
            {
                var errorMessage = $"Failed to deactivate DID: {result.Errors.FirstOrDefault()?.Message}";
                actionOutcome.FinishOutcomeWithFailure(errorMessage);
                return Result.Fail(errorMessage);
            }

            if (result.Value.DidState.State == "failed")
            {
                var errorMessage = $"Failed to deactivate DID on the OPN Registrar: {result.Value.DidState.Reason}";
                actionOutcome.FinishOutcomeWithFailure(errorMessage);
                return Result.Fail(errorMessage);
            }

            var jobId = result.Value.JobId;
            if (string.IsNullOrEmpty(jobId))
            {
                var errorMessage = $"Failed to deactivate DID: OPN Registrar returned an invalid job ID";
                actionOutcome.FinishOutcomeWithFailure(errorMessage);
                return Result.Fail(errorMessage);
            }

            // --- Start of Polling Implementation ---
            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.Elapsed < TimeoutLimit)
            {
                // Check for cancellation request
                context.CancellationToken.ThrowIfCancellationRequested();

                var jobStatus = await client.GetDeactivateResultAsync(registrarUrl, did, jobId, context.CancellationToken);

                if (jobStatus.IsFailed)
                {
                    // Log the error but continue polling unless it's a permanent failure
                    var errorMessage = $"Failed to get job status: {jobStatus.Errors.FirstOrDefault()?.Message}";
                    actionOutcome.FinishOutcomeWithFailure(errorMessage);
                    stopwatch.Stop();
                    return Result.Fail(errorMessage);
                }

                var currentState = jobStatus.Value.DidState?.State;

                if (currentState == "finished")
                {
                    var resultString = JsonSerializer.Serialize(jobStatus.Value.DidState, new JsonSerializerOptions() { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });
                    actionOutcome.FinishOutcomeWithSuccess(resultString);
                    stopwatch.Stop();
                    return Result.Ok();
                }

                if (currentState == "failed")
                {
                    var errorMessage = $"Failed deactivation operation: {jobStatus.Value.DidState?.Reason ?? "No reason provided."}";
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
                    // Handle cases where the state might be missing, maybe log a warning but continue polling.
                    Console.WriteLine($"Warning: Job status state is null or empty for job ID {jobId}. Continuing poll.");
                }

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
            var timeoutErrorMessage = $"DID deactivation status check timed out after {TimeoutLimit.TotalMinutes} minutes for job ID {jobId}.";
            actionOutcome.FinishOutcomeWithFailure(timeoutErrorMessage);
            return Result.Fail(timeoutErrorMessage);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error processing DeactivateDID action: {ex.Message}";
            actionOutcome.FinishOutcomeWithFailure(errorMessage);
            return Result.Fail(errorMessage);
        }
    }
}