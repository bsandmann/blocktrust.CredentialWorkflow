namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow;

using System.Text.Json;
using Domain.Common;
using Domain.Credential;
using Domain.Enums;
using Domain.ProcessFlow.Actions;
using Domain.ProcessFlow.Actions.Issuance;
using Domain.ProcessFlow.Triggers;
using Domain.Workflow;
using Entities.Outcome;
using FluentResults;
using GetWorkflowById;
using GetWorkflows;
using IssueCredentials.IssueW3cCredential.CreateW3cCredential;
using IssueCredentials.IssueW3cCredential.SignW3cCredential;
using MediatR;
using Org.BouncyCastle.Bcpg.Sig;
using Tenant.GetIssuingKeys;
using Tenant.GetIssungKeyById;
using Tenant.GetPrivateIssuingKeyByDid;
using WorkflowOutcome.UpdateWorkflowOutcome;

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
        var workflow = await _mediator.Send(new GetWorkflowByIdRequest(workflowId), cancellationToken);
        if (workflow.IsFailed)
        {
            return Result.Fail("Unable to laod Workflow");
        }

        ExecutionContext executionContext = BuildExecutionContext(workflow.Value, executionContextString);

        if (workflow.Value.ProcessFlow is null)
        {
            return Result.Fail("Unable to process Workflow. Now processflow definition");
        }

        // TODO ensure the correct order of actions
        var actionOutcomes = new List<ActionOutcome>();
        foreach (var action in workflow.Value.ProcessFlow!.Actions)
        {
            var actionId = action.Key;
            var actionType = action.Value.Type;
            var actionInput = action.Value.Input;
            var actionOutcome = new ActionOutcome(actionId);

            if (actionType == EActionType.IssueW3CCredential)
            {
                var input = (IssueW3cCredential)actionInput;
                var subjectDid = await GetParameterFromExecutionContext(input.SubjectDid, executionContext);
                if (subjectDid == null)
                {
                    var errorMessage = "The subject DID is not provided in the execution context parameters.";
                    return await FinishActionsWithFailure(workflowOutcomeId, actionOutcome, errorMessage, actionOutcomes, cancellationToken);
                }

                var issuerDid = await GetParameterFromExecutionContext(input.IssuerDid, executionContext);
                if (issuerDid == null)
                {
                    var errorMessage = "The issuer DID is not provided.";
                    return await FinishActionsWithFailure(workflowOutcomeId, actionOutcome, errorMessage, actionOutcomes, cancellationToken);
                }

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
                    var errorMessage = "he W3C credential could not be created.";
                    return await FinishActionsWithFailure(workflowOutcomeId, actionOutcome, errorMessage, actionOutcomes, cancellationToken);
                }

                var issuignKeyResult = await _mediator.Send(new GetPrivateIssuingKeyByDidRequest(issuerDid), cancellationToken);
                if (issuignKeyResult.IsFailed)
                {
                    var errorMessage = "The private key for the issuer DID could not be found.";
                    return await FinishActionsWithFailure(workflowOutcomeId, actionOutcome, errorMessage, actionOutcomes, cancellationToken);
                }

                var privatekeyResult = HexStringToByteArray(issuignKeyResult.Value);
                if (privatekeyResult.IsFailed)
                {
                    var errorMessage = privatekeyResult.Errors.FirstOrDefault()?.Message ?? "The private key for the issuer DID could not be parsed.";
                    return await FinishActionsWithFailure(workflowOutcomeId, actionOutcome, errorMessage, actionOutcomes, cancellationToken);
                }

                var signedCredentialRequest = new SignW3cCredentialRequest(
                    credential: createW3CCredentialResult.Value,
                    issuerDid: issuerDid,
                    privateKey: privatekeyResult.Value);
                var signedCredentialResult = await _mediator.Send(signedCredentialRequest, cancellationToken);
                if (signedCredentialResult.IsFailed)
                {
                    var errorMessage = privatekeyResult.Errors.FirstOrDefault()?.Message ?? "The credential could not be signed.";
                    return await FinishActionsWithFailure(workflowOutcomeId, actionOutcome, errorMessage, actionOutcomes, cancellationToken);
                }

                var successString = signedCredentialResult.Value;
                actionOutcome.FinishOutcomeWithSuccess(successString);
                actionOutcomes.Add(actionOutcome);
            }
            else
            {
                return Result.Fail($"The action type {actionType} is not supported.");
            }
        }

        return await FinishActionsWithSuccess(workflowOutcomeId, actionOutcomes, cancellationToken);
    }

    private async Task<Result<bool>> FinishActionsWithSuccess(Guid workflowOutcomeId, List<ActionOutcome> actionOutcomes, CancellationToken cancellationToken)
    {
        var workflowUpdateResult = await _mediator.Send(
            new UpdateWorkflowOutcomeRequest(
                workflowOutcomeId,
                EWorkflowOutcomeState.Success, JsonSerializer.Serialize(actionOutcomes), null), cancellationToken);
        if (workflowUpdateResult.IsFailed)
        {
            return Result.Fail("The workflow outcome could not be updated.");
        }

        return Result.Ok(true);
    }


    private async Task<Result<bool>> FinishActionsWithFailure(Guid worflowOutcomeId, ActionOutcome actionOutcome, string errorMessage, List<ActionOutcome> actionOutcomes, CancellationToken cancellationToken)
    {
        actionOutcome.FinishOutcomeWithFailure(errorMessage);
        actionOutcomes.Add(actionOutcome);
        var workflowUpdateResult = await _mediator.Send(
            new UpdateWorkflowOutcomeRequest(
                worflowOutcomeId,
                EWorkflowOutcomeState.FailedWithErrors, JsonSerializer.Serialize(actionOutcomes), errorMessage), cancellationToken);
        if (workflowUpdateResult.IsFailed)
        {
            return Result.Fail("The workflow outcome could not be updated.");
        }

        return Result.Ok(false);
    }


    private ExecutionContext BuildExecutionContext(Workflow workflow, string? executionContextString)
    {
        var trigger = workflow?.ProcessFlow?.Triggers.First().Value;
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

    private async Task<string?> GetParameterFromExecutionContext(ParameterReference parameterReference, ExecutionContext executionContext)
    {
        if (parameterReference.Source == ParameterSource.TriggerInput)
        {
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
        }
        else if (parameterReference.Source == ParameterSource.AppSettings)
        {
            // TODO currently the only thing saved in the AppSettings (don't confuse it with the appsettings.json) is the IssuingKeys

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

                if (issuingKey.IssuingKeyId.Equals(keyId)) ;
                {
                    return issuingKey.Did;
                }
            }
        }

        return null;
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

    // TODO refactor this to be placed somewhere where we actually save the private keys

    public static Result<byte[]> HexStringToByteArray(string hex)
    {
        if (hex.Length % 2 != 0)
        {
            return Result.Fail("Hex string must have an even number of characters.");
        }

        var byteCount = hex.Length / 2;
        var bytes = new byte[byteCount];

        for (int i = 0; i < byteCount; i++)
        {
            // Grab two characters from the string
            string twoChars = hex.Substring(i * 2, 2);
            // Convert the two characters into a byte
            bytes[i] = Convert.ToByte(twoChars, 16);
        }

        return Result.Ok(bytes);
    }
}