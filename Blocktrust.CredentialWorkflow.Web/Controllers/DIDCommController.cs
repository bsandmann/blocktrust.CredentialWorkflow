using System.Text;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.GetWorkflowById;
using Blocktrust.CredentialWorkflow.Core.Domain.Enums;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Triggers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blocktrust.CredentialWorkflow.Web.Controllers;

using System.Text.Json;
using Blocktrust.Common.Models.DidDoc;
using Blocktrust.Common.Resolver;
using Common;
using Core.Commands.DIDComm.ProcessMessage;
using Core.Prism;
using DIDComm;
using DIDComm.Common.Types;
using DIDComm.Message.Attachments;
using DIDComm.Message.Messages;
using DIDComm.Model.PackEncryptedParamsModels;
using DIDComm.Model.UnpackParamsModels;
using Mediator.Client.Commands.ForwardMessage;
using Mediator.Common;
using Mediator.Common.Commands.CreatePeerDid;
using Mediator.Common.Models.CredentialOffer;
using Mediator.Common.Models.OutOfBand;
using Mediator.Common.Protocols;
using Net.Codecrete.QrCodeGenerator;

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
    /// DIDComm workflow status check endpoint - Returns configuration information for DIDComm
    /// </summary>
    /// <param name="workflowGuidId">The ID of the workflow to check</param>
    /// <returns>The configured PeerDID for the workflow</returns>
    [HttpGet("api/workflow/{workflowGuidId:Guid}/didcomm")]
    public async Task<ActionResult<object>> DIDCommStatusEndpoint(Guid workflowGuidId)
    {
        var getWorkflowRequest = new GetWorkflowByIdRequest(workflowGuidId);
        var getWorkflowResult = await _mediator.Send(getWorkflowRequest);
        if (getWorkflowResult.IsFailed)
        {
            return BadRequest(new { success = false, message = "Workflow not found", details = getWorkflowResult.Errors });
        }

        if (getWorkflowResult.Value.WorkflowState == EWorkflowState.Inactive)
        {
            return Ok(new { success = false, message = "The workflow is inactive" });
        }

        // Check if the workflow has triggers
        var processFlow = getWorkflowResult.Value.ProcessFlow;
        if (processFlow is null || !processFlow.Triggers.Any())
        {
            return Ok(new { success = false, message = "The workflow does not have a trigger" });
        }

        // Ensure that the trigger is a DIDComm request trigger
        var trigger = processFlow.Triggers.First().Value;
        if (trigger.Type != ETriggerType.WalletInteraction)
        {
            return Ok(new { success = false, message = "The workflow trigger is not a DIDComm (WalletInteraction) trigger" });
        }

        // Extract the PeerDID from the WalletInteractionTrigger
        var triggerInput = (TriggerInputWalletInteraction)trigger.Input;
        var workflowPlatformPeerDid = triggerInput.PeerDid;
        if (string.IsNullOrEmpty(workflowPlatformPeerDid))
        {
            return Ok(new { success = false, message = "No PeerDID configured for this workflow" });
        }

        return Ok(workflowPlatformPeerDid);
    }

    /// <summary>
    /// DIDComm workflow status check endpoint - Returns configuration information for DIDComm
    /// </summary>
    /// <param name="workflowGuidId">The ID of the workflow to check</param>
    /// <returns>The configured PeerDID for the workflow</returns>
    [HttpGet("api/workflow/{workflowGuidId:Guid}/didcomm/oob")]
    public async Task<ActionResult> DIDCommStatusEndpointOob(Guid workflowGuidId)
    {
        var getWorkflowRequest = new GetWorkflowByIdRequest(workflowGuidId);
        var getWorkflowResult = await _mediator.Send(getWorkflowRequest);
        if (getWorkflowResult.IsFailed)
        {
            return BadRequest(new { success = false, message = "Workflow not found", details = getWorkflowResult.Errors });
        }

        if (getWorkflowResult.Value.WorkflowState == EWorkflowState.Inactive)
        {
            return Ok(new { success = false, message = "The workflow is inactive" });
        }

        // Check if the workflow has triggers
        var processFlow = getWorkflowResult.Value.ProcessFlow;
        if (processFlow is null || !processFlow.Triggers.Any())
        {
            return Ok(new { success = false, message = "The workflow does not have a trigger" });
        }

        // Ensure that the trigger is a DIDComm request trigger
        var trigger = processFlow.Triggers.First().Value;
        if (trigger.Type != ETriggerType.WalletInteraction)
        {
            return Ok(new { success = false, message = "The workflow trigger is not a DIDComm (WalletInteraction) trigger" });
        }

        // Extract the PeerDID from the WalletInteractionTrigger
        var triggerInput = (TriggerInputWalletInteraction)trigger.Input;
        var workflowPlatformPeerDid = triggerInput.PeerDid;
        if (string.IsNullOrEmpty(workflowPlatformPeerDid))
        {
            return Ok(new { success = false, message = "No PeerDID configured for this workflow" });
        }

        var hostUrl = string.Concat(_httpContextAccessor!.HttpContext.Request.Scheme, "://", _httpContextAccessor.HttpContext.Request.Host);
        var msg = new OobModel()
        {
            Type = ProtocolConstants.OutOfBand2Invitation,
            Id = Guid.NewGuid().ToString(),
            From = workflowPlatformPeerDid,
            Body = new OobBodyModel()
            {
                GoalCode = "connect",
                Goal = "Request connection through the blocktrust workflow platform",
                Accept = new List<string>() { "didcomm/v2" },
                Label = "Blocktrust Workflow Platform",
            },
            Attachments = null
        };
        var jsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IgnoreNullValues = true
        };
        var json = JsonSerializer.Serialize(msg, jsonSerializerOptions);
        var base64Url = PrismEncoding.ByteArrayToBase64(Encoding.UTF8.GetBytes(json));

        var invitationUrl = string.Concat(hostUrl, "?_oob=", base64Url);

        var qr = QrCode.EncodeText(invitationUrl, QrCode.Ecc.Medium);
        string svg = qr.ToSvgString(4);

        // Generate HTML using helper method
        string htmlContent = GenerateOutOfBandInvitationHtml(invitationUrl, svg);
        // --- End HTML Generation ---

        // Return as HTML content
        return new ContentResult
        {
            Content = htmlContent,
            ContentType = "text/html",
            StatusCode = StatusCodes.Status200OK // Microsoft.AspNetCore.Http.StatusCodes
        };
    }

    /// <summary>
    /// Send a presentation request through DIDComm
    /// </summary>
    /// <param name="workflowGuidId">The workflow ID to use for the request</param>
    /// <param name="requestData">Request data containing the sender PeerDID</param>
    /// <returns>Result of the operation</returns>
    [HttpPost("api/workflow/{workflowGuidId:Guid}/didcomm/presentation-request")]
    public async Task<ActionResult> DIDCommPresentationRequest(Guid workflowGuidId, [FromBody] JsonElement requestData)
    {
        // Validate the request has the required field
        if (!requestData.TryGetProperty("senderPeerDid", out var senderPeerDidElement) ||
            senderPeerDidElement.ValueKind != JsonValueKind.String ||
            string.IsNullOrEmpty(senderPeerDidElement.GetString()))
        {
            return BadRequest(new { success = false, message = "Request must include 'senderPeerDid' field" });
        }

        string senderPeerDid = senderPeerDidElement.GetString()!;
        DidDoc? senderPeerDidDoc = null;
        try
        {
            senderPeerDidDoc = await _didDocResolver.Resolve(senderPeerDid);
        }
        catch (Exception e)
        {
            return BadRequest(new { success = false, message = "Invalid PeerDID" });
        }

        if (senderPeerDidDoc is null)
        {
            return BadRequest(new { success = false, message = "Invalid PeerDID" });
        }

        // Get the workflow
        var getWorkflowRequest = new GetWorkflowByIdRequest(workflowGuidId);
        var getWorkflowResult = await _mediator.Send(getWorkflowRequest);
        if (getWorkflowResult.IsFailed)
        {
            return BadRequest(new { success = false, message = "Workflow not found", details = getWorkflowResult.Errors });
        }

        if (getWorkflowResult.Value.WorkflowState == EWorkflowState.Inactive)
        {
            return BadRequest(new { success = false, message = "The workflow is inactive" });
        }

        // Check if the workflow has triggers
        var processFlow = getWorkflowResult.Value.ProcessFlow;
        if (processFlow is null || !processFlow.Triggers.Any())
        {
            return BadRequest(new { success = false, message = "The workflow does not have a trigger" });
        }

        // Ensure that the trigger is a DIDComm request trigger
        var trigger = processFlow.Triggers.First().Value;
        if (trigger.Type != ETriggerType.WalletInteraction)
        {
            return BadRequest(new { success = false, message = "The workflow trigger is not a DIDComm (WalletInteraction) trigger" });
        }

        // Extract the PeerDID from the WalletInteractionTrigger
        var triggerInput = (TriggerInputWalletInteraction)trigger.Input;

        // Ensure that the trigger is configured for CredentialPresentation
        if (triggerInput.MessageType != "CredentialPresentation")
        {
            return BadRequest(new
            {
                success = false,
                message = "The workflow trigger is not configured for credential presentations",
                currentMessageType = triggerInput.MessageType
            });
        }

        var workflowPlatformPeerDid = triggerInput.PeerDid;
        if (string.IsNullOrEmpty(workflowPlatformPeerDid))
        {
            return BadRequest(new { success = false, message = "No PeerDID configured for this workflow" });
        }


        var presentation = new Presentation()
        {
            PresentationRequest = new PresentationRequest()
            {
                Domain = "blocktrust-wallet ",
                Challenge = Guid.NewGuid().ToString(),
                Nonce = Guid.NewGuid().ToString()
            },
            PresentationDefinition = new PresentationDefinition()
            {
                Format = null,
                Name = null,
                Purpose = null,
                Id = Guid.NewGuid().ToString(),
            }
        };

        var presentationJsonString = JsonSerializer.Serialize(presentation);
        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(presentationJsonString);

        var body = new Dictionary<string, object>();
        body.Add("goal_code", GoalCodes.PrismPresentationRequest);
        body.Add("comment", null);
        body.Add("will_confirm", true);
        body.Add("formats", new List<string>());
        var attachment = new AttachmentBuilder(Guid.NewGuid().ToString(), new Json(dict)).Build();
        var message = new MessageBuilder(
                id: Guid.NewGuid().ToString(),
                type: ProtocolConstants.PresentProofPresentationRequest,
                body: body
            )
            .thid(Guid.NewGuid().ToString())
            .from(workflowPlatformPeerDid)
            .to(new List<string>() { senderPeerDid })
            .attachments(new List<Attachment>()
            {
                attachment
            })
            .build();

        var didComm = new DidComm(_didDocResolver, _secretResolver);
        var packResult = await didComm.PackEncrypted(
            new PackEncryptedParamsBuilder(message, to: senderPeerDid)
                .From(workflowPlatformPeerDid)
                .ProtectSenderId(false)
                .BuildPackEncryptedParams()
        );

        if (packResult.IsFailed)
        {
            return BadRequest($"Unable to pack message: {packResult.Errors.First().Message}");
        }

        var didDocSenderDid = await _didDocResolver.Resolve(senderPeerDid);
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
                recipientDid: senderPeerDid
            ));

            if (forwardMessageResult.IsFailed)
            {
                var errorMessage = $"The Forward-Message could not be sent: {forwardMessageResult.Errors.FirstOrDefault()?.Message}";
                return BadRequest(errorMessage);
            }

            return Ok();
        }
    }

    /// <summary>
    /// DIDComm workflow endpoint for receiving messages
    /// </summary>
    /// <param name="workflowGuidId">The workflow ID</param>
    /// <returns>Response to the DIDComm message</returns>
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
    
    /// <summary>
    /// Generates a styled HTML page for displaying DIDComm Out-of-band invitation with QR code
    /// </summary>
    /// <param name="invitationUrl">The invitation URL to be displayed and encoded in the QR code</param>
    /// <param name="svg">The SVG string representation of the QR code</param>
    /// <returns>An HTML string with Tailwind-style formatting</returns>
    private string GenerateOutOfBandInvitationHtml(string invitationUrl, string svg)
    {
        // Using raw string literal for better readability of HTML template
        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>DIDComm Out-of-band invitation</title>
    <script src=""https://cdn.tailwindcss.com""></script>
</head>
<body class=""bg-gray-50 text-gray-900"">
    <div class=""min-h-screen p-4 pt-12 flex justify-center"">
        <div class=""max-w-md w-full bg-white rounded-lg shadow-md p-6 space-y-6"">
            <div class=""text-center"">
                <h1 class=""text-2xl font-bold text-slate-800 mb-4"">DIDComm Out-of-band invitation</h1>
                
                <p class=""text-gray-600 mb-4"">Scan the QR Code below or use the following invitation URL:</p>
                
                <div class=""bg-gray-100 rounded p-3 mb-6 overflow-auto text-sm text-gray-800 break-all"">
                    <a href=""{System.Net.WebUtility.HtmlEncode(invitationUrl)}"" 
                       target=""_blank"" 
                       class=""text-blue-600 hover:text-blue-800 hover:underline"">
                        {System.Net.WebUtility.HtmlEncode(invitationUrl)}
                    </a>
                </div>
                
                <div class=""mx-auto"" style=""max-width:400px"">
                    {svg}
                </div>
            </div>
        </div>
    </div>
</body>
</html>";
    }
}