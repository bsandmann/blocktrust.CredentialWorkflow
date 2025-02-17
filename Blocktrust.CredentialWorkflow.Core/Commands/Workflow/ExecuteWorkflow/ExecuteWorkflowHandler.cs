using System.Text.Json;
using System.Text.RegularExpressions;
using Blocktrust.CredentialWorkflow.Core.Commands.IssueCredentials.IssueW3cCredential.CreateW3cCredential;
using Blocktrust.CredentialWorkflow.Core.Commands.IssueCredentials.IssueW3cCredential.SignW3cCredential;
using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.GetIssuingKeys;
using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.GetPrivateIssuingKeyByDid;
using Blocktrust.CredentialWorkflow.Core.Commands.VerifyCredentials.VerifyW3cCredentials.VerifyW3cCredential;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.GetWorkflowById;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.SendEmailAction;
using Blocktrust.CredentialWorkflow.Core.Commands.WorkflowOutcome.UpdateWorkflowOutcome;
using Blocktrust.CredentialWorkflow.Core.Domain.Common;
using Blocktrust.CredentialWorkflow.Core.Domain.Enums;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Issuance;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Outgoing;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Verification;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Triggers;
using Blocktrust.CredentialWorkflow.Core.Domain.Workflow;
using Blocktrust.VerifiableCredential.Common;
using FluentResults;
using MediatR;
using Action = Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Action;
using ExecutionContext = Blocktrust.CredentialWorkflow.Core.Domain.Common.ExecutionContext;

public class ExecuteWorkflowHandler : IRequestHandler<ExecuteWorkflowRequest, Result<bool>>
{
    private readonly IMediator _mediator;

    public ExecuteWorkflowHandler(IMediator mediator)
    {
        _mediator = mediator;
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
            // 1) Find the next action:
            //    - If previousActionId is null, we look for an action whose runAfter references the triggerId with EFlowStatus.Succeeded
            //    - If previousActionId is set, we look for an action whose runAfter references that previousActionId with EFlowStatus.Succeeded
            var nextActionKvp = FindNextAction(
                workflow.ProcessFlow.Actions,
                triggerId,
                previousActionId
            );

            if (nextActionKvp is null)
            {
                // No further action found => we’re done. Break out of the loop and finalize with success.
                break;
            }

            // // 2) Check if the next action references a 'Failed' predecessor. If so, end the workflow with failure.
            // if (HasFailedPredecessor(nextActionKvp.Value.Value.RunAfter))
            // {
            //     // The workflow should fail immediately if a predecessor was marked as Failed
            //     var failedOutcome = new ActionOutcome(nextActionKvp.Value.Key);
            //     failedOutcome.FinishOutcomeWithFailure("A predecessor was failed. Ending workflow.");
            //     actionOutcomes.Add(failedOutcome);
            //
            //     return await FinishActionsWithFailure(
            //         workflowOutcomeId,
            //         failedOutcome,
            //         "A predecessor was failed. No further processing.",
            //         actionOutcomes,
            //         cancellationToken
            //     );
            // }

            // 3) Process the action
            var actionId = nextActionKvp.Value.Key;
            var action = nextActionKvp.Value.Value;

            var actionOutcome = new ActionOutcome(actionId);
            switch (action.Type)
            {
                case EActionType.IssueW3CCredential:
                {
                    var result = await ProcessIssueW3CCredentialAction(
                        action,
                        actionOutcome,
                        workflowOutcomeId,
                        executionContext,
                        actionOutcomes,
                        workflow,
                        cancellationToken
                    );
                    if (result.IsFailed || result.Value.Equals(false))
                    {
                        // Already finished with failure inside the method
                        return result;
                    }

                    // If we got here, we succeeded for this action
                    break;
                }
                case EActionType.VerifyW3CCredential:
                {
                    var result = await ProcessVerifyW3CCredentialAction(
                        action,
                        actionOutcome,
                        workflowOutcomeId,
                        executionContext,
                        actionOutcomes,
                        workflow,
                        cancellationToken
                    );
                    if (result.IsFailed || result.Value.Equals(false))
                    {
                        // Already finished with failure inside the method
                        return result;
                    }

                    break;
                }
                case EActionType.Email:
                {
                    var result = await ProcessEmailAction(
                        action,
                        actionOutcome,
                        workflowOutcomeId,
                        executionContext,
                        actionOutcomes,
                        workflow,
                        cancellationToken
                    );
                    if (result.IsFailed || result.Value.Equals(false))
                    {
                        return result;
                    }
                    break;
                }
                default:
                {
                    return Result.Fail($"The action type {action.Type} is not supported.");
                }
            }

            // 4) Add success outcome for this action
            actionOutcomes.Add(actionOutcome);

            // 5) Move forward to the next iteration
            //    Mark the "previous action" as this one (so next look-up references this action's ID)
            previousActionId = actionId;
        }

        // If we exited the loop normally, finalize with success:
        return await FinishActionsWithSuccess(workflowOutcomeId, actionOutcomes, cancellationToken);
    }

    /// <summary>
    /// Finds the first action that references either:
    ///   - the triggerId (if previousActionId == null)
    ///   - the previousActionId (otherwise)
    /// in its RunAfter dictionary with EFlowStatus == Succeeded.
    /// Returns null if no matching action is found.
    /// </summary>
    private KeyValuePair<Guid, Action>? FindNextAction(
        Dictionary<Guid, Action> actions,
        Guid triggerId,
        Guid? previousActionId
    )
    {
        // The ID we must match in the RunAfter dictionary
        var predecessorId = previousActionId ?? triggerId;

        // SingleOrDefault to find a unique action that references predecessorId with Succeeded
        var nextAction = actions
            .SingleOrDefault(x => x.Value.RunAfter.Count == 1
                                  && x.Value.RunAfter.Single() == predecessorId);

        // If Key == default(Guid), it means SingleOrDefault found nothing
        if (nextAction.Key == default && nextAction.Value == null)
        {
            // No action found
            return null;
        }

        return nextAction;
    }

    /// <summary>
    /// Processes the IssueW3CCredential action and updates the given actionOutcome upon success/failure.
    /// Returns a Result indicating if the action was successful or not. If unsuccessful, it already finishes the workflow with failure.
    /// </summary>
    private async Task<Result<bool>> ProcessIssueW3CCredentialAction(
        Action action,
        ActionOutcome actionOutcome,
        Guid workflowOutcomeId,
        ExecutionContext executionContext,
        List<ActionOutcome> actionOutcomes,
        Workflow workflow,
        CancellationToken cancellationToken
    )
    {
        var input = (IssueW3cCredential)action.Input;

        var subjectDid = await GetParameterFromExecutionContext(input.SubjectDid, executionContext, workflow, actionOutcomes);
        if (subjectDid == null)
        {
            var errorMessage = "The subject DID is not provided in the execution context parameters.";
            return await FinishActionsWithFailure(workflowOutcomeId, actionOutcome, errorMessage, actionOutcomes, cancellationToken);
        }

        var issuerDid = await GetParameterFromExecutionContext(input.IssuerDid, executionContext, workflow, actionOutcomes);
        if (issuerDid == null)
        {
            var errorMessage = "The issuer DID is not provided.";
            return await FinishActionsWithFailure(workflowOutcomeId, actionOutcome, errorMessage, actionOutcomes, cancellationToken);
        }

        // 1) Create W3C credential
        var createW3CCredentialRequest = new CreateW3cCredentialRequest(
            issuerDid: issuerDid,
            subjectDid: subjectDid,
            additionalSubjectData: GetClaimsFromExecutionContext(input.Claims, executionContext),
            validFrom: null,
            expirationDate: null
        );

        var createW3CCredentialResult = await _mediator.Send(createW3CCredentialRequest, cancellationToken);
        if (createW3CCredentialResult.IsFailed)
        {
            var errorMessage = "The W3C credential could not be created.";
            return await FinishActionsWithFailure(workflowOutcomeId, actionOutcome, errorMessage, actionOutcomes, cancellationToken);
        }

        // 2) Retrieve private key
        var issuignKeyResult = await _mediator.Send(new GetPrivateIssuingKeyByDidRequest(issuerDid), cancellationToken);
        if (issuignKeyResult.IsFailed)
        {
            var errorMessage = "The private key for the issuer DID could not be found.";
            return await FinishActionsWithFailure(workflowOutcomeId, actionOutcome, errorMessage, actionOutcomes, cancellationToken);
        }

        byte[] privatekeyResult;
        try
        {
            privatekeyResult = Base64Url.Decode(issuignKeyResult.Value);
        }
        catch (Exception e)
        {
            var errorMessage = "The private key for the issuer DID could not be parsed: " + e.Message;
            return await FinishActionsWithFailure(workflowOutcomeId, actionOutcome, errorMessage, actionOutcomes, cancellationToken);
        }

        // 3) Sign W3C credential
        var signedCredentialRequest = new SignW3cCredentialRequest(
            credential: createW3CCredentialResult.Value,
            issuerDid: issuerDid,
            privateKey: privatekeyResult
        );

        var signedCredentialResult = await _mediator.Send(signedCredentialRequest, cancellationToken);
        if (signedCredentialResult.IsFailed)
        {
            var errorMessage = signedCredentialResult.Errors.FirstOrDefault()?.Message
                               ?? "The credential could not be signed.";
            return await FinishActionsWithFailure(workflowOutcomeId, actionOutcome, errorMessage, actionOutcomes, cancellationToken);
        }

        var successString = signedCredentialResult.Value;
        actionOutcome.FinishOutcomeWithSuccess(successString);

        return Result.Ok(true);
    }

    /// <summary>
    /// Processes the VerifyW3CCredential action logic:
    ///   - Retrieve the credential from execution context
    ///   - Send VerifyW3CCredentialRequest
    ///   - Record success/failure in the actionOutcome
    /// </summary>
    private async Task<Result<bool>> ProcessVerifyW3CCredentialAction(
        Action action,
        ActionOutcome actionOutcome,
        Guid workflowOutcomeId,
        ExecutionContext executionContext,
        List<ActionOutcome> actionOutcomes,
        Workflow workflow,
        CancellationToken cancellationToken
    )
    {
        var input = (VerifyW3cCredential)action.Input;

        // Get the credential as string from context
        var credentialStr = await GetParameterFromExecutionContext(input.CredentialReference, executionContext, workflow, actionOutcomes);
        if (string.IsNullOrWhiteSpace(credentialStr))
        {
            var errorMessage = "No credential found in the execution context to verify.";
            return await FinishActionsWithFailure(workflowOutcomeId, actionOutcome, errorMessage, actionOutcomes, cancellationToken);
        }

        // Send the verification request
        var verifyRequest = new VerifyW3CCredentialRequest(credentialStr, input.CheckSignature, input.CheckExpiry, input.CheckRevocationStatus, input.CheckSchema, input.CheckTrustRegistry);
        var verifyResult = await _mediator.Send(verifyRequest, cancellationToken);
        if (verifyResult.IsFailed)
        {
            var errorMessage = verifyResult.Errors.FirstOrDefault()?.Message
                               ?? "Verification failed.";
            return await FinishActionsWithFailure(workflowOutcomeId, actionOutcome, errorMessage, actionOutcomes, cancellationToken);
        }

        var verificationResult = verifyResult.Value;
        var successString = $"Credential verified. IsValid={verificationResult.IsValid}";
        actionOutcome.FinishOutcomeWithSuccess(successString);

        return Result.Ok(true);
    }
    


private async Task<Result<bool>> ProcessEmailAction(
    Action action,
    ActionOutcome actionOutcome,
    Guid workflowOutcomeId,
    ExecutionContext executionContext,
    List<ActionOutcome> actionOutcomes,
    Workflow workflow,
    CancellationToken cancellationToken
)
{
    var input = (EmailAction)action.Input;

    // Get the recipient email
    var toEmail = await GetParameterFromExecutionContext(input.To, executionContext, workflow, actionOutcomes);
    if (string.IsNullOrWhiteSpace(toEmail))
    {
        var errorMessage = "The recipient email address is not provided in the execution context parameters.";
        return await FinishActionsWithFailure(workflowOutcomeId, actionOutcome, errorMessage, actionOutcomes, cancellationToken);
    }

    // Process parameters
    var parameters = new Dictionary<string, string>();
    foreach (var param in input.Parameters)
    {
        var value = await GetParameterFromExecutionContext(param.Value, executionContext, workflow, actionOutcomes);
        if (value != null)
        {
            parameters[param.Key] = value;
        }
    }

    try
    {
        // Process templates
        var subject = ProcessEmailTemplate(input.Subject, parameters);
        var body = ProcessEmailTemplate(input.Body, parameters);

        // Send the email
        var sendEmailRequest = new SendEmailActionRequest(toEmail, subject, body);
        var sendResult = await _mediator.Send(sendEmailRequest, cancellationToken);
        
        if (sendResult.IsFailed)
        {
            var errorMessage = sendResult.Errors.FirstOrDefault()?.Message ?? "Failed to send email.";
            return await FinishActionsWithFailure(workflowOutcomeId, actionOutcome, errorMessage, actionOutcomes, cancellationToken);
        }

        actionOutcome.FinishOutcomeWithSuccess("Email sent successfully");
        return Result.Ok(true);
    }
    catch (Exception ex)
    {
        var errorMessage = $"Error sending email: {ex.Message}";
        return await FinishActionsWithFailure(workflowOutcomeId, actionOutcome, errorMessage, actionOutcomes, cancellationToken);
    }
}

public string ProcessEmailTemplate(string template, Dictionary<string, string> parameters)
{
    // Return empty string if template is null/empty
    if (string.IsNullOrEmpty(template))
        return string.Empty;
        
    // If no parameters, return original template
    if (parameters == null || !parameters.Any())
        return template;

    var processedTemplate = template;
    foreach (var param in parameters)
    {
        // Ensure key is not null before replacing
        if (!string.IsNullOrEmpty(param.Key))
        {
            // Handle null values by replacing with empty string
            var paramValue = param.Value ?? string.Empty;
            
            // Trim the key to handle any extra whitespace
            var key = param.Key.Trim();
            
            // Make replacement case-insensitive
            processedTemplate = Regex.Replace(
                processedTemplate, 
                $"\\[{key}\\]", 
                paramValue, 
                RegexOptions.IgnoreCase);
        }
    }

    return processedTemplate;
}
private async Task<Result<bool>> FinishActionsWithSuccess(Guid workflowOutcomeId, List<ActionOutcome> actionOutcomes, CancellationToken cancellationToken)
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

    private ExecutionContext BuildExecutionContext(Workflow workflow, string? executionContextString)
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

        return new ExecutionContext(workflow!.TenantId);
    }

private async Task<string?> GetParameterFromExecutionContext(ParameterReference parameterReference, ExecutionContext executionContext, Workflow workflow, List<ActionOutcome> actionOutcomes)
{
    switch (parameterReference.Source)
    {
        case ParameterSource.Static:
            return parameterReference.Path;  // For static values, return the path directly

        case ParameterSource.TriggerInput:
            if (executionContext.InputContext is null)
            {
                return null;
            }

            var exists = executionContext.InputContext.TryGetValue(parameterReference.Path.ToLowerInvariant(), out var value);
            if (!exists)
            {
                return null;
            }

            return value;

        case ParameterSource.AppSettings:
            // For example, to get an Issuing DID from "AppSettings"
            var issuingKeys = await _mediator.Send(new GetIssuingKeysRequest(executionContext.TenantId));
            if (issuingKeys.IsFailed)
            {
                return null;
            }

            foreach (var issuingKey in issuingKeys.Value)
            {
                var isParseable = Guid.TryParse(parameterReference.Path, out var keyId);
                if (!isParseable)
                {
                    return null;
                }

                if (issuingKey.IssuingKeyId.Equals(keyId))
                {
                    return issuingKey.Did;
                }
            }
            return null;

        case ParameterSource.ActionOutcome:
            var actionId = parameterReference.ActionId;
            var referencedActionExists = workflow.ProcessFlow!.Actions.Any(p => p.Key.Equals(actionId));
            if (!referencedActionExists)
            {
                return null;
            }

            var referencedAction = workflow.ProcessFlow.Actions.FirstOrDefault(p => p.Key.Equals(actionId));
            switch (referencedAction.Value.Type)
            {
                case EActionType.IssueW3CCredential:
                    var actionOutcome = actionOutcomes.FirstOrDefault(p => p.ActionId.Equals(actionId));
                    if (actionOutcome is null)
                    {
                        return null;
                    }

                    return actionOutcome.OutcomeJson;
                default:
                    return null;
            }

        default:
            return null;
    }
}
    private Dictionary<string, object>? GetClaimsFromExecutionContext(Dictionary<string, ClaimValue> inputClaims, ExecutionContext executionContext)
    {
        var claims = new Dictionary<string, object>();
        foreach (var inputClaim in inputClaims)
        {
            var key = inputClaim.Key;
            if (inputClaim.Value.Type == ClaimValueType.Static)
            {
                claims.TryAdd(key, inputClaim.Value.Value);
            }
            else if (inputClaim.Value.Type == ClaimValueType.TriggerProperty)
            {
                if (executionContext.InputContext is null)
                {
                    continue;
                }

                if (inputClaim.Value.ParameterReference is null)
                {
                    continue;
                }

                var exists = executionContext.InputContext.TryGetValue(inputClaim.Value.ParameterReference.Path.ToLowerInvariant(), out var value);
                if (!exists || value is null)
                {
                    continue;
                }

                claims.TryAdd(key, value);
            }
            else
            {
                continue;
            }
        }

        return claims;
    }
}


