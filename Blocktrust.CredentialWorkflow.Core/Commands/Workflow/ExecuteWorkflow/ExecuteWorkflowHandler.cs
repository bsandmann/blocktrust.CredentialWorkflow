using System.Text.Json;
using Blocktrust.Common.Resolver;
using Blocktrust.CredentialWorkflow.Core.Commands.IssueCredentials.IssueW3cCredential.CreateW3cCredential;
using Blocktrust.CredentialWorkflow.Core.Commands.IssueCredentials.IssueW3cCredential.SignW3cCredential;
using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.GetIssuingKeys;
using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.GetPrivateIssuingKeyByDid;
using Blocktrust.CredentialWorkflow.Core.Commands.VerifyCredentials.VerifyW3cCredentials.VerifyW3cCredential;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.GetWorkflowById;
using Blocktrust.CredentialWorkflow.Core.Commands.WorkflowOutcome.UpdateWorkflowOutcome;
using Blocktrust.CredentialWorkflow.Core.Domain.Common;
using Blocktrust.CredentialWorkflow.Core.Domain.Enums;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Issuance;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Outgoing;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Verification;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Triggers;
using Blocktrust.Mediator.Client.Commands.SendMessage;
using Blocktrust.Mediator.Common.Commands.CreatePeerDid;
using Blocktrust.Mediator.Common.Protocols;
using Blocktrust.PeerDID.DIDDoc;
using Blocktrust.PeerDID.PeerDIDCreateResolve;
using Blocktrust.PeerDID.Types;
using Blocktrust.VerifiableCredential.Common;
using FluentResults;
using MediatR;
using Action = Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Action;
using ExecutionContext = Blocktrust.CredentialWorkflow.Core.Domain.Common.ExecutionContext;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow;

using System.Text;
using Blocktrust.DIDComm.Message.Attachments;
using Blocktrust.DIDComm.Message.Messages;
using DIDComm.GetPeerDIDs;
using Mediator.Client.Commands.ForwardMessage;
using Mediator.Client.Commands.TrustPing;
using Mediator.Common;
using Mediator.Common.Models.CredentialOffer;
using Workflow = Domain.Workflow.Workflow;

public class ExecuteWorkflowHandler : IRequestHandler<ExecuteWorkflowRequest, Result<bool>>
{
    private readonly IMediator _mediator;
    private readonly ISecretResolver _secretResolver;
    private readonly IDidDocResolver _didDocResolver;

    public ExecuteWorkflowHandler(IMediator mediator, ISecretResolver secretResolver, IDidDocResolver didDocResolver)
    {
        _mediator = mediator;
        _secretResolver = secretResolver;
        _didDocResolver = didDocResolver;
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
                case EActionType.DIDComm:
                {
                    var result = await ProcessDIDCommAction(
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

    private async Task<Result<bool>> ProcessDIDCommAction(Action action, ActionOutcome actionOutcome, Guid workflowOutcomeId, ExecutionContext executionContext, List<ActionOutcome> actionOutcomes, Workflow workflow, CancellationToken cancellationToken)
    {
        var input = (DIDCommAction)action.Input;

        var recipientPeerDid = await GetParameterFromExecutionContext(input.RecipientPeerDid, executionContext, workflow, actionOutcomes, EActionType.DIDComm);
        if (recipientPeerDid == null)
        {
            var errorMessage = "The recipient Peer-DID is not provided in the execution context parameters.";
            return await FinishActionsWithFailure(workflowOutcomeId, actionOutcome, errorMessage, actionOutcomes, cancellationToken);
        }

        var recipientPeerDidResult = PeerDidResolver.ResolvePeerDid(new PeerDid(recipientPeerDid), VerificationMaterialFormatPeerDid.Jwk);
        if (recipientPeerDidResult.IsFailed)
        {
            var errorMessage = "The recipient Peer-DID could not be resolved: " + recipientPeerDidResult.Errors.FirstOrDefault()?.Message;
            return await FinishActionsWithFailure(workflowOutcomeId, actionOutcome, errorMessage, actionOutcomes, cancellationToken);
        }

        var recipientPeerDidDocResult = DidDocPeerDid.FromJson(recipientPeerDidResult.Value);
        if (recipientPeerDidDocResult.IsFailed)
        {
            var errorMessage = "The recipient Peer-DID could bot be translated into a valid DID-doc:  " + recipientPeerDidDocResult.Errors.FirstOrDefault()?.Message;
            return await FinishActionsWithFailure(workflowOutcomeId, actionOutcome, errorMessage, actionOutcomes, cancellationToken);
        }

        if (recipientPeerDidDocResult.Value.Services is null || recipientPeerDidDocResult.Value.Services.Count == 0 || recipientPeerDidDocResult.Value.Services.FirstOrDefault()!.ServiceEndpoint is null ||
            string.IsNullOrWhiteSpace(recipientPeerDidDocResult.Value.Services.FirstOrDefault().ServiceEndpoint.Uri))
        {
            var errorMessage = "The recipient Peer-DID does not have a valid service endpoint.";
            return await FinishActionsWithFailure(workflowOutcomeId, actionOutcome, errorMessage, actionOutcomes, cancellationToken);
        }

        var tenantDids = await _mediator.Send(new GetPeerDIDsRequest(workflow.TenantId));
        if (tenantDids.IsFailed)
        {
            var errorMessage = "The local Peer-DIDs of the tenant could not be fetched: " + tenantDids.Errors.FirstOrDefault()?.Message;
            return await FinishActionsWithFailure(workflowOutcomeId, actionOutcome, errorMessage, actionOutcomes, cancellationToken);
        }

        var localDid = await GetParameterFromExecutionContext(input.SenderPeerDid, executionContext, workflow, actionOutcomes, EActionType.DIDComm);
        if (string.IsNullOrWhiteSpace(localDid))
        {
            var errorMessage = "The local Peer-DIDs of the tenant could not be identified: " + tenantDids.Errors.FirstOrDefault()?.Message;
            return await FinishActionsWithFailure(workflowOutcomeId, actionOutcome, errorMessage, actionOutcomes, cancellationToken);
        }

        var endpoint = recipientPeerDidDocResult.Value.Services.First().ServiceEndpoint.Uri;
        var did = recipientPeerDidDocResult.Value.Did;
        if (endpoint.StartsWith("did:peer"))
        {
            var innerPeerDid = PeerDidResolver.ResolvePeerDid(new PeerDid(endpoint), VerificationMaterialFormatPeerDid.Jwk);
            var innerPeerDidDocResult = DidDocPeerDid.FromJson(innerPeerDid.Value);
            endpoint = innerPeerDidDocResult.Value.Services.First().ServiceEndpoint.Uri;
            did = recipientPeerDidDocResult.Value.Services.First().ServiceEndpoint.Uri;
        }

        if (input.Type == EDIDCommType.Message)
        {
            var basicMessage = BasicMessage.Create("Hello you!", localDid);
            var packedBasicMessage = await BasicMessage.Pack(basicMessage, from: localDid, recipientPeerDid, _secretResolver, _didDocResolver);

            var forwardMessageResult = await _mediator.Send(new SendForwardMessageRequest(
                message: packedBasicMessage.Value,
                localDid: localDid,
                mediatorDid: did,
                mediatorEndpoint: new Uri(endpoint),
                recipientDid: recipientPeerDid
            ), cancellationToken);
            if (forwardMessageResult.IsFailed)
            {
                var errorMessage = "The Forward-Message could not be sent: " + forwardMessageResult.Errors.FirstOrDefault()?.Message;
                return await FinishActionsWithFailure(workflowOutcomeId, actionOutcome, errorMessage, actionOutcomes, cancellationToken);
            }
        }
        else if (input.Type == EDIDCommType.TrustPing)
        {
            var trustPingRequest = new TrustPingRequest(new Uri(endpoint), did, localDid, suggestedLabel: "TrustPing");
            var trustPingResult = await _mediator.Send(trustPingRequest, cancellationToken);
            if (trustPingResult.IsFailed)
            {
                var errorMessage = "The TrustPing request could not be sent: " + trustPingResult.Errors.FirstOrDefault()?.Message;
                return await FinishActionsWithFailure(workflowOutcomeId, actionOutcome, errorMessage, actionOutcomes, cancellationToken);
            }
        }
        else if (input.Type == EDIDCommType.CredentialIssuance)
        {
            // Get the credential as string from context
            var credentialStr = await GetParameterFromExecutionContext(input.CredentialReference, executionContext, workflow, actionOutcomes, EActionType.DIDComm);
            if (string.IsNullOrWhiteSpace(credentialStr))
            {
                var errorMessage = "No credential found in the execution context to verify.";
                return await FinishActionsWithFailure(workflowOutcomeId, actionOutcome, errorMessage, actionOutcomes, cancellationToken);
            }

            var encodedCredential = Base64Url.Encode(Encoding.UTF8.GetBytes(credentialStr));

            var msg = BuildIssuingMessage(new PeerDid(localDid), new PeerDid(recipientPeerDid), Guid.NewGuid().ToString(), encodedCredential);
            var packedBasicMessage = await BasicMessage.Pack(msg, from: localDid, recipientPeerDid, _secretResolver, _didDocResolver);
            if (packedBasicMessage.IsFailed)
            {
                var errorMessage = "Failed to pack the message: " + packedBasicMessage.Errors.FirstOrDefault()?.Message;
                return await FinishActionsWithFailure(workflowOutcomeId, actionOutcome, errorMessage, actionOutcomes, cancellationToken);
            }

            var forwardMessageResult = await _mediator.Send(new SendForwardMessageRequest(
                message: packedBasicMessage.Value,
                localDid: localDid,
                mediatorDid: did,
                mediatorEndpoint: new Uri(endpoint),
                recipientDid: recipientPeerDid
            ), cancellationToken);
            if (forwardMessageResult.IsFailed)
            {
                var errorMessage = "The Forward-Message could not be sent: " + forwardMessageResult.Errors.FirstOrDefault()?.Message;
                return await FinishActionsWithFailure(workflowOutcomeId, actionOutcome, errorMessage, actionOutcomes, cancellationToken);
            }
        }
        else
        {
            var errorMessage = "The recipient Peer-DID is not provided in the execution context parameters.";
            return await FinishActionsWithFailure(workflowOutcomeId, actionOutcome, errorMessage, actionOutcomes, cancellationToken);
        }

        var successString = "Message sent successfully.";
        actionOutcome.FinishOutcomeWithSuccess(successString);

        return Result.Ok(true);
    }


    private static Message BuildIssuingMessage(PeerDid localPeerDid, PeerDid prismPeerDid, string messageId, string signedCredential)
    {
        var unixTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var body = new Dictionary<string, object>
        {
            { "goal_code", GoalCodes.PrismCredentialOffer },
            { "comment", null! },
            { "formats", new List<string>() }
        };
        var attachment = new AttachmentBuilder(Guid.NewGuid().ToString(), new Base64(signedCredential))
            .Build();
        var responseMessage = new MessageBuilder(
                id: Guid.NewGuid().ToString(),
                type: ProtocolConstants.IssueCredential2Issue,
                body: body
            )
            .thid(messageId)
            .from(localPeerDid.Value)
            .to(new List<string>() { prismPeerDid.Value })
            .attachments(new List<Attachment>()
            {
                attachment
            })
            .createdTime(unixTimeStamp)
            .expiresTime(unixTimeStamp + 1000)
            .build();
        return responseMessage;
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

        var subjectDid = await GetParameterFromExecutionContext(input.SubjectDid, executionContext, workflow, actionOutcomes, EActionType.IssueW3CCredential);
        if (subjectDid == null)
        {
            var errorMessage = "The subject DID is not provided in the execution context parameters.";
            return await FinishActionsWithFailure(workflowOutcomeId, actionOutcome, errorMessage, actionOutcomes, cancellationToken);
        }

        var issuerDid = await GetParameterFromExecutionContext(input.IssuerDid, executionContext, workflow, actionOutcomes, EActionType.IssueW3CCredential);
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
        var credentialStr = await GetParameterFromExecutionContext(input.CredentialReference, executionContext, workflow, actionOutcomes, EActionType.VerifyW3CCredential);
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

    private async Task<string?> GetParameterFromExecutionContext(ParameterReference parameterReference, ExecutionContext executionContext, Workflow workflow, List<ActionOutcome> actionOutcomes, EActionType? actionType)
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
            if (actionType == EActionType.IssueW3CCredential)
            {
                var issuingKeys = await _mediator.Send(new GetIssuingKeysRequest(executionContext.TenantId));
                if (issuingKeys.IsFailed || issuingKeys.Value is null || issuingKeys.Value.Count == 0)
                {
                    return null;
                }

                if (parameterReference.Path.Equals("DefaultIssuerDid", StringComparison.CurrentCultureIgnoreCase))
                {
                    return issuingKeys.Value.First().Did;
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
            }
            else if (actionType == EActionType.DIDComm)
            {
                var peerDids = await _mediator.Send(new GetPeerDIDsRequest(executionContext.TenantId));
                if (peerDids.IsFailed || peerDids.Value is null || peerDids.Value.Count == 0)
                {
                    return null;
                }

                if (parameterReference.Path.Equals("DefaultSenderDid", StringComparison.CurrentCultureIgnoreCase))
                {
                    return peerDids.Value.First().PeerDID;
                }

                foreach (var peerDid in peerDids.Value)
                {
                    var isParseable = Guid.TryParse(parameterReference.Path, out var keyId);
                    if (!isParseable)
                    {
                        return null;
                    }

                    if (peerDid.PeerDIDEntityId.Equals(keyId))
                    {
                        return peerDid.PeerDID;
                    }
                }
            }
            else
            {
                return null;
            }
        }
        else if (parameterReference.Source == ParameterSource.ActionOutcome)
        {
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

                    var credential = actionOutcome.OutcomeJson;
                    return credential;
                    break;
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
}