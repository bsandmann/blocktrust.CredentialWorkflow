using System.Text;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.GetWorkflowById;
using Blocktrust.CredentialWorkflow.Core.Commands.WorkflowOutcome.CreateWorkflowOutcome;
using Blocktrust.CredentialWorkflow.Core.Domain.Common;
using Blocktrust.CredentialWorkflow.Core.Domain.Enums;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Triggers;
using Blocktrust.CredentialWorkflow.Core.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blocktrust.CredentialWorkflow.Web.Controllers;

using System.Text.Json;
using Blocktrust.Common.Resolver;
using Common;
using Core.Commands.DIDComm.ProcessMessage;
using DIDComm;
using DIDComm.Common.Types;
using DIDComm.Model.PackEncryptedParamsModels;
using DIDComm.Model.UnpackParamsModels;
using FluentResults;
using Mediator.Client.Commands.ForwardMessage;

[ApiController]
[AllowAnonymous]
public class DIDCommController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISecretResolver _secretResolver;
    private readonly IDidDocResolver _didDocResolver;

    public DIDCommController(IMediator mediator, IHttpContextAccessor httpContextAccessor, ISecretResolver secretResolver, IDidDocResolver didDocResolver, HttpClient httpClient)
    {
        _mediator = mediator;
        _httpContextAccessor = httpContextAccessor;
        _secretResolver = secretResolver;
        _didDocResolver = didDocResolver;
        _httpClient = httpClient;
    }

    /// <summary>
    /// DIDComm workflow endpoint
    /// </summary>
    /// <returns></returns>
    [HttpPost("api/workflow/{workflowGuidId:Guid}/didcomm")]
    public async Task<ActionResult<string>> DIDCommTriggerEndpoint(Guid workflowGuidId)
    {
        var getWorkflowRequest = new GetWorkflowByIdRequest(workflowGuidId);
        var getWorkflowResult = await _mediator.Send(getWorkflowRequest);
        if (getWorkflowResult.IsFailed)
        {
            return BadRequest(getWorkflowResult.Errors);
        }

        if (getWorkflowResult.Value.WorkflowState == EWorkflowState.Inactive)
        {
            return BadRequest("The workflow is inactive");
        }

        var workflow = getWorkflowResult.Value;

        // Check if the workflow has triggers
        var processFlow = getWorkflowResult.Value.ProcessFlow;
        if (processFlow is null || !processFlow.Triggers.Any())
        {
            return BadRequest("The workflow does not have a trigger");
        }

        // Ensure that the trigger is an DIDComm request trigger
        var trigger = processFlow.Triggers.First().Value;
        if (trigger.Type != ETriggerType.WalletInteraction)
        {
            return BadRequest("The workflow trigger is not an DIDComm (WalletInteraction) request trigger");
        }

        // Extract the PeerDID from the WalletInteractionTrigger
        var triggerInput = (TriggerInputWalletInteraction)trigger.Input;
        var workflowPlatformPeerDid = triggerInput.PeerDid;
        if (string.IsNullOrEmpty(workflowPlatformPeerDid))
        {
            return BadRequest("The recipientPeerDid could not be found in the configuration");
        }

        var hostUrl = string.Concat(_httpContextAccessor!.HttpContext.Request.Scheme, "://", _httpContextAccessor.HttpContext.Request.Host);

        var request = _httpContextAccessor.HttpContext.Request;
        var body = await new StreamReader(request.Body).ReadToEndAsync();

        var didComm = new DidComm(_didDocResolver, _secretResolver);
        var unpacked = await didComm.Unpack(
            new UnpackParamsBuilder(body).BuildUnpackParams()
        );
        if (unpacked.IsFailed)
        {
            return BadRequest($"Unable to unpack message: {unpacked.Errors.First().Message}");
        }

        string? senderOldDid = null;
        string? senderDid = null;

        if (unpacked.Value.Message.FromPrior is not null)
        {
            senderOldDid = unpacked.Value.Message.FromPrior.Iss;
            senderDid = unpacked.Value.Message.FromPrior.Sub;
        }
        else
        {
            var encryptedFrom = unpacked.Value.Metadata.EncryptedFrom;
            if (encryptedFrom is null)
            {
                // Should only be really the case for forward messages
                senderDid = null;
                senderOldDid = null;
            }
            else
            {
                var split = encryptedFrom.Split("#");
                senderDid = split.First();
                senderOldDid = senderDid;
            }
        }

        var processMessageResponse = await _mediator.Send(new ProcessMessageRequest(senderOldDid, senderDid, hostUrl, unpacked.Value, workflow));

        // Check if we have a return route flag. Otherwise we should send a separate message
        var customHeaders = unpacked.Value.Message.CustomHeaders;
        var returnRouteHeader = unpacked.Value.Message.ReturnRoute;
        EnumReturnRoute returnRoute = EnumReturnRoute.None;
        if ((customHeaders.TryGetValue("return_route", out var returnRouteString)))
        {
            var returnRouteJsonElement = (JsonElement)returnRouteString;
            if (returnRouteJsonElement.ValueKind == JsonValueKind.String)
            {
                Enum.TryParse(returnRouteJsonElement.GetString(), true, out returnRoute);
            }
        }
        else if (returnRouteHeader is not null && returnRouteHeader.Equals("all", StringComparison.InvariantCultureIgnoreCase))
        {
            returnRoute = EnumReturnRoute.All;
        }

        //TODO in some cases I might want to respond with a empty-message instead or accepted or a defined response. Figure out where

        // TODO simplification: the correct use of 'thid' here should be tested
        if (returnRoute == EnumReturnRoute.All || (returnRoute == EnumReturnRoute.Thread && processMessageResponse.Message.Thid is not null && processMessageResponse.Message!.Thid!.Equals(unpacked.Value.Message.Thid)))
        {
            if (processMessageResponse.RespondWithAccepted || senderDid is null)
            {
                return Accepted();
            }

            var packResult = await didComm.PackEncrypted(
                new PackEncryptedParamsBuilder(processMessageResponse.Message, to: senderDid)
                    .From(workflowPlatformPeerDid)
                    .ProtectSenderId(false)
                    .BuildPackEncryptedParams()
            );

            if (packResult.IsFailed)
            {
                return BadRequest($"Unable to pack message: {packResult.Errors.First().Message}");
            }

            return Ok(packResult.Value.PackedMessage);
        }
        else
        {
            if (processMessageResponse.RespondWithAccepted || senderDid is null)
            {
                //TODO I should fall back to an empty message here

                return Accepted();
            }

            var packResult = await didComm.PackEncrypted(
                new PackEncryptedParamsBuilder(processMessageResponse.Message, to: senderDid)
                    .From(workflowPlatformPeerDid)
                    .ProtectSenderId(false)
                    .BuildPackEncryptedParams()
            );

            if (packResult.IsFailed)
            {
                return BadRequest($"Unable to pack message: {packResult.Errors.First().Message}");
            }

            var didDocSenderDid = await _didDocResolver.Resolve(senderDid);
            if (didDocSenderDid is null)
            {
                return Ok(packResult.Value.PackedMessage);
            }

            var service = didDocSenderDid.Services.FirstOrDefault();
            if (service is null)
            {
                // Fallback to just sending a http-response
                return Ok(packResult.Value.PackedMessage);
            }

            var endpoint = service!.ServiceEndpoint;
            if (string.IsNullOrEmpty(endpoint.Uri))
            {
                // Fallback to just sending a http-response
                return Ok(packResult.Value.PackedMessage);
            }

            var hasMediatorEndpoint = false;

            var isUri = Uri.TryCreate(endpoint.Uri, UriKind.Absolute, out var endpointUri);
            if (!isUri)
            {
                // Fallback to just sending a http-response
                return Ok(packResult.Value.PackedMessage);
            }

            if (endpoint.Uri.StartsWith("did:peer", StringComparison.InvariantCultureIgnoreCase))
            {
                var endpointResolved = await _didDocResolver.Resolve(endpoint.Uri);
                if (endpointResolved is null)
                {
                    // Fallback to just sending a http-response
                    return Ok(packResult.Value.PackedMessage);
                }

                var endpointService = endpointResolved.Services.FirstOrDefault();
                {
                    if (endpointService is null)
                    {
                        // Fallback to just sending a http-response
                        return Ok(packResult.Value.PackedMessage);
                    }
                }

                if (string.IsNullOrEmpty(endpointService.ServiceEndpoint.Uri))
                {
                    // Fallback to just sending a http-response
                    return Ok(packResult.Value.PackedMessage);
                }

                var isEndpointUri = Uri.TryCreate(endpointService.ServiceEndpoint.Uri, UriKind.Absolute, out endpointUri);
                if (!isEndpointUri)
                {
                    // Fallback to just sending a http-response
                    return Ok(packResult.Value.PackedMessage);
                }

                hasMediatorEndpoint = true;
            }

            if (!hasMediatorEndpoint)
            {
                try
                {
                    // Upgrade scheme from http to https if applicable
                    // if (endpointUri.Scheme == "http")
                    // {
                    //     endpointUri = new Uri($"https://{endpointUri.Authority}{endpointUri.PathAndQuery}");
                    // }

                    var response = await _httpClient.PostAsync(endpointUri,
                        new StringContent(packResult.Value.PackedMessage, Encoding.UTF8, MessageTyp.Encrypted));
                    if (!response.IsSuccessStatusCode)
                    {
                        // Fallback to just sending a http-response
                        return Ok(packResult.Value.PackedMessage);
                    }

                    return Ok();
                }
                catch (Exception e)
                {
                    return BadRequest($"Error sending message back to endpoint: '{endpointUri.AbsoluteUri}'. Error: {e.Message}");
                }
            }
            else
            {
                var forwardMessageResult = await _mediator.Send(new SendForwardMessageRequest(
                    message: packResult.Value.PackedMessage,
                    localDid: workflowPlatformPeerDid,
                    mediatorDid: endpoint.Uri,
                    mediatorEndpoint: endpointUri,
                    recipientDid: senderDid
                ));

                if (forwardMessageResult.IsFailed)
                {
                    var errorMessage = $"The Forward-Message could not be sent: {forwardMessageResult.Errors.FirstOrDefault()?.Message}";
                    return BadRequest(errorMessage);
                }

                return Ok();
            }
        }
    }
}