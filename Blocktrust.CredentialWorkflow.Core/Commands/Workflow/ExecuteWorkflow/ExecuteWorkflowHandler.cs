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


using System.Text;
using Blocktrust.DIDComm.Message.Attachments;
using Blocktrust.DIDComm.Message.Messages;
using DIDComm.GetPeerDIDs;
using Mediator.Client.Commands.ForwardMessage;
using Mediator.Client.Commands.TrustPing;
using Mediator.Common;
using Mediator.Common.Models.CredentialOffer;
using Workflow = Domain.Workflow.Workflow;
namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow;

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
            var nextActionKvp = FindNextAction(
                workflow.ProcessFlow.Actions,
                triggerId,
                previousActionId
            );

            if (nextActionKvp is null)
            {
                break;
            }

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
                        return result;
                    }
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
                        return result;
                    }
                    break;
                }
                default:
                {
                    return Result.Fail($"The action type {action.Type} is not supported.");
                }
            }

            actionOutcomes.Add(actionOutcome);
            previousActionId = actionId;
        }

        return await FinishActionsWithSuccess(workflowOutcomeId, actionOutcomes, cancellationToken);
    }

    private KeyValuePair<Guid, Action>? FindNextAction(
        Dictionary<Guid, Action> actions,
        Guid triggerId,
        Guid? previousActionId
    )
    {
        var predecessorId = previousActionId ?? triggerId;

        var nextAction = actions
            .SingleOrDefault(x => x.Value.RunAfter.Count == 1
                                  && x.Value.RunAfter.Single() == predecessorId);

        if (nextAction.Key == default && nextAction.Value == null)
        {
            return null;
        }

        return nextAction;
    }

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

        var credentialStr = await GetParameterFromExecutionContext(input.CredentialReference, executionContext, workflow, actionOutcomes, EActionType.VerifyW3CCredential);
        if (string.IsNullOrWhiteSpace(credentialStr))
        {
            var errorMessage = "No credential found in the execution context to verify.";
            return await FinishActionsWithFailure(workflowOutcomeId, actionOutcome, errorMessage, actionOutcomes, cancellationToken);
        }

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

        var toEmail = await GetParameterFromExecutionContext(input.To, executionContext, workflow, actionOutcomes);
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            var errorMessage = "The recipient email address is not provided in the execution context parameters.";
            return await FinishActionsWithFailure(workflowOutcomeId, actionOutcome, errorMessage, actionOutcomes, cancellationToken);
        }

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
            var subject = ProcessEmailTemplate(input.Subject, parameters);
var body = ProcessEmailTemplate(input.Body, parameters);

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

    private async Task<Result<bool>> ProcessDIDCommAction(
        Action action,
        ActionOutcome actionOutcome,
        Guid workflowOutcomeId,
        ExecutionContext executionContext,
        List<ActionOutcome> actionOutcomes,
        Workflow workflow,
        CancellationToken cancellationToken
    )
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

    private string ProcessEmailTemplate(string template, Dictionary<string, string> parameters)
    {
        if (string.IsNullOrEmpty(template))
            return string.Empty;
            
        if (parameters == null || !parameters.Any())
            return template;

        var processedTemplate = template;
        foreach (var param in parameters)
        {
            if (!string.IsNullOrEmpty(param.Key))
            {
                var paramValue = param.Value ?? string.Empty;
                var key = param.Key.Trim();
                processedTemplate = Regex.Replace(
                    processedTemplate, 
                    $"\\[{key}\\]", 
                    paramValue, 
                    RegexOptions.IgnoreCase);
            }
        }

        return processedTemplate;
    }

    private async Task<Result<bool>> FinishActionsWithSuccess(
        Guid workflowOutcomeId, 
        List<ActionOutcome> actionOutcomes, 
        CancellationToken cancellationToken)
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

    private ExecutionContext BuildExecutionContext(Domain.Workflow.Workflow workflow, string? executionContextString)
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

    private async Task<string?> GetParameterFromExecutionContext(
        ParameterReference parameterReference, 
        ExecutionContext executionContext, 
        Workflow workflow, 
        List<ActionOutcome> actionOutcomes, 
        EActionType? actionType = null)
    {
        switch (parameterReference.Source)
        {
            case ParameterSource.Static:
                return parameterReference.Path;

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