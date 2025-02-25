using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Outgoing;
using Blocktrust.DIDComm.Message.Attachments;
using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.Mediator.Client.Commands.ForwardMessage;
using Blocktrust.Mediator.Client.Commands.TrustPing;
using Blocktrust.Mediator.Common.Protocols;
using Blocktrust.PeerDID.DIDDoc;
using Blocktrust.PeerDID.PeerDIDCreateResolve;
using Blocktrust.PeerDID.Types;
using FluentResults;
using MediatR;
using System.Text;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow.ActionProcessors;

using Common.Resolver;
using Mediator.Common;
using VerifiableCredential.Common;
using Action = Domain.ProcessFlow.Actions.Action;

public class DIDCommActionProcessor : IActionProcessor
{
    private readonly IMediator _mediator;
    private readonly ISecretResolver _secretResolver;
    private readonly IDidDocResolver _didDocResolver;

    public DIDCommActionProcessor(IMediator mediator, ISecretResolver secretResolver, IDidDocResolver didDocResolver)
    {
        _mediator = mediator;
        _secretResolver = secretResolver;
        _didDocResolver = didDocResolver;
    }

    public EActionType ActionType => EActionType.DIDComm;

    public async Task<Result> ProcessAsync(Action action, ActionOutcome actionOutcome, ActionProcessingContext context)
    {
        var input = (DIDCommAction)action.Input;

        var recipientPeerDid = await ParameterResolver.GetParameterFromExecutionContext(
            input.RecipientPeerDid, context.ExecutionContext, context.Workflow, context.ActionOutcomes, ActionType, _mediator);
        if (recipientPeerDid == null)
        {
            var errorMessage = "The recipient Peer-DID is not provided in the execution context parameters.";
            actionOutcome.FinishOutcomeWithFailure(errorMessage);
            return Result.Fail(errorMessage);
        }

        var recipientPeerDidResult = PeerDidResolver.ResolvePeerDid(new PeerDid(recipientPeerDid), VerificationMaterialFormatPeerDid.Jwk);
        if (recipientPeerDidResult.IsFailed)
        {
            var errorMessage = $"The recipient Peer-DID could not be resolved: {recipientPeerDidResult.Errors.FirstOrDefault()?.Message}";
            actionOutcome.FinishOutcomeWithFailure(errorMessage);
            return Result.Fail(errorMessage);
        }

        var recipientPeerDidDocResult = DidDocPeerDid.FromJson(recipientPeerDidResult.Value);
        if (recipientPeerDidDocResult.IsFailed || recipientPeerDidDocResult.Value.Services is null || recipientPeerDidDocResult.Value.Services.Count == 0)
        {
            var errorMessage = "The recipient Peer-DID does not have a valid service endpoint.";
            actionOutcome.FinishOutcomeWithFailure(errorMessage);
            return Result.Fail(errorMessage);
        }

        var localDid = await ParameterResolver.GetParameterFromExecutionContext(
            input.SenderPeerDid, context.ExecutionContext, context.Workflow, context.ActionOutcomes, ActionType, _mediator);
        if (string.IsNullOrWhiteSpace(localDid))
        {
            var errorMessage = "The local Peer-DID could not be identified.";
            actionOutcome.FinishOutcomeWithFailure(errorMessage);
            return Result.Fail(errorMessage);
        }

        var endpoint = recipientPeerDidDocResult.Value.Services.First().ServiceEndpoint.Uri;
        var did = recipientPeerDidDocResult.Value.Did;
        if (endpoint.StartsWith("did:peer"))
        {
            var innerPeerDidResult = PeerDidResolver.ResolvePeerDid(new PeerDid(endpoint), VerificationMaterialFormatPeerDid.Jwk);
            var innerPeerDidDocResult = DidDocPeerDid.FromJson(innerPeerDidResult.Value);
            endpoint = innerPeerDidDocResult.Value.Services.First().ServiceEndpoint.Uri;
            did = innerPeerDidDocResult.Value.Did;
        }

        if (input.Type == EDIDCommType.Message)
        {
            var basicMessage = BasicMessage.Create("Hello!", localDid);
            var packedBasicMessage = await BasicMessage.Pack(basicMessage, from: localDid, finalRecipientDid: recipientPeerDid, _secretResolver, _didDocResolver);
            if (packedBasicMessage.IsFailed)
            {
                var errorMessage = $"Failed to pack the message: {packedBasicMessage.Errors.FirstOrDefault()?.Message}";
                actionOutcome.FinishOutcomeWithFailure(errorMessage);
                return Result.Fail(errorMessage);
            }

            var forwardMessageResult = await _mediator.Send(new SendForwardMessageRequest(
                message: packedBasicMessage.Value,
                localDid: localDid,
                mediatorDid: did,
                mediatorEndpoint: new Uri(endpoint),
                recipientDid: recipientPeerDid
            ), context.CancellationToken);

            if (forwardMessageResult.IsFailed)
            {
                var errorMessage = $"The Forward-Message could not be sent: {forwardMessageResult.Errors.FirstOrDefault()?.Message}";
                actionOutcome.FinishOutcomeWithFailure(errorMessage);
                return Result.Fail(errorMessage);
            }
        }
        else if (input.Type == EDIDCommType.TrustPing)
        {
            var trustPingRequest = new TrustPingRequest(new Uri(endpoint), did, localDid, suggestedLabel: "TrustPing");
            var trustPingResult = await _mediator.Send(trustPingRequest, context.CancellationToken);
            if (trustPingResult.IsFailed)
            {
                var errorMessage = $"The TrustPing request could not be sent: {trustPingResult.Errors.FirstOrDefault()?.Message}";
                actionOutcome.FinishOutcomeWithFailure(errorMessage);
                return Result.Fail(errorMessage);
            }
        }
        else if (input.Type == EDIDCommType.CredentialIssuance)
        {
            var credentialStr = await ParameterResolver.GetParameterFromExecutionContext(
                input.CredentialReference, context.ExecutionContext, context.Workflow, context.ActionOutcomes, ActionType, _mediator);
            if (string.IsNullOrWhiteSpace(credentialStr))
            {
                var errorMessage = "No credential found in the execution context to issue.";
                actionOutcome.FinishOutcomeWithFailure(errorMessage);
                return Result.Fail(errorMessage);
            }

            var encodedCredential = Base64Url.Encode(Encoding.UTF8.GetBytes(credentialStr));

            var msg = BuildIssuingMessage(new PeerDid(localDid), new PeerDid(recipientPeerDid), Guid.NewGuid().ToString(), encodedCredential);
            var packedMessage = await BasicMessage.Pack(msg, from: localDid, finalRecipientDid: recipientPeerDid, _secretResolver, _didDocResolver);
            if (packedMessage.IsFailed)
            {
                var errorMessage = $"Failed to pack the credential issuance message: {packedMessage.Errors.FirstOrDefault()?.Message}";
                actionOutcome.FinishOutcomeWithFailure(errorMessage);
                return Result.Fail(errorMessage);
            }

            var forwardMessageResult = await _mediator.Send(new SendForwardMessageRequest(
                message: packedMessage.Value,
                localDid: localDid,
                mediatorDid: did,
                mediatorEndpoint: new Uri(endpoint),
                recipientDid: recipientPeerDid
            ), context.CancellationToken);

            if (forwardMessageResult.IsFailed)
            {
                var errorMessage = $"The Forward-Message for credential issuance could not be sent: {forwardMessageResult.Errors.FirstOrDefault()?.Message}";
                actionOutcome.FinishOutcomeWithFailure(errorMessage);
                return Result.Fail(errorMessage);
            }
        }
        else
        {
            var errorMessage = $"Unsupported DIDComm type: {input.Type}";
            actionOutcome.FinishOutcomeWithFailure(errorMessage);
            return Result.Fail(errorMessage);
        }

        actionOutcome.FinishOutcomeWithSuccess("DIDComm action executed successfully.");
        return Result.Ok();
    }

    private Message BuildIssuingMessage(PeerDid localDid, PeerDid recipientDid, string messageId, string encodedCredential)
    {
        var unixTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var body = new Dictionary<string, object>
        {
            { "goal_code", GoalCodes.PrismCredentialOffer },
            { "comment", null },
            { "formats", new List<string>() }
        };
        var attachment = new AttachmentBuilder(Guid.NewGuid().ToString(), new Base64(encodedCredential)).Build();
        return new MessageBuilder(
                id: Guid.NewGuid().ToString(),
                type: ProtocolConstants.IssueCredential2Issue,
                body: body
            )
            .thid(messageId)
            .from(localDid.Value)
            .to(new List<string> { recipientDid.Value })
            .attachments(new List<Attachment> { attachment })
            .createdTime(unixTimeStamp)
            .expiresTime(unixTimeStamp + 1000)
            .build();
    }
}