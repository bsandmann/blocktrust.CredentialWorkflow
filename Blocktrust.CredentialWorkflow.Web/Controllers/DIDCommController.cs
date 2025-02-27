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

using Blocktrust.Common.Resolver;
using DIDComm;
using DIDComm.Model.UnpackParamsModels;

[ApiController]
[AllowAnonymous]
public class DIDCommController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IWorkflowQueue _workflowQueue;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISecretResolver _secretResolver;
    private readonly IDidDocResolver _didDocResolver;

    public DIDCommController(IMediator mediator, WorkflowQueue workflowQueue, IHttpContextAccessor httpContextAccessor, ISecretResolver secretResolver, IDidDocResolver didDocResolver)
    {
        _mediator = mediator;
        _workflowQueue = workflowQueue;
        _httpContextAccessor = httpContextAccessor;
        _secretResolver = secretResolver;
        _didDocResolver = didDocResolver;
    }

    /// <summary>
    /// DIDComm address endpoint
    /// </summary>
    /// <returns></returns>
    [HttpGet("api/workflow/{workflowGuidId:guid}/connect")]
    public async Task<ActionResult<string>> DIDCommConnectionEndpoint(Guid workflowGuidId)
    {
        var hostUrl = string.Concat(_httpContextAccessor!.HttpContext.Request.Scheme, "://", _httpContextAccessor.HttpContext.Request.Host);

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


        // Maybe generate a peer did or invite (?) as a recipient endpoint


        // var existingInvitationResult = await _mediator.Send(new GetOobInvitationRequest(hostUrl));
        // var invitation = string.Empty;
        // if (existingInvitationResult.IsFailed)
        // {
        //     var peerDidResponse = await _mediator.Send(new CreatePeerDidRequest(numberOfAgreementKeys: 1, numberOfAuthenticationKeys: 1, serviceEndpoint: new Uri(hostUrl)));
        //     if (peerDidResponse.IsFailed)
        //     {
        //         return Problem(statusCode: 500, detail: peerDidResponse.Errors.First().Message);
        //     }
        //
        //     var result = await _mediator.Send(new CreateOobInvitationRequest(hostUrl, peerDidResponse.Value.PeerDid));
        //     if (result.IsFailed)
        //     {
        //         return Problem(statusCode: 500, detail: result.Errors.First().Message);
        //     }
        //
        //     invitation = result.Value.Invitation;
        // }
        // else
        // {
        //     invitation = existingInvitationResult.Value.Invitation;
        // }
        //
        // var invitationUrl = string.Concat(hostUrl, "?_oob=", invitation);
        // return Ok(invitationUrl);

        return Ok();
    }

    /// <summary>
    /// DIDComm workflow endpoint
    /// </summary>
    /// <returns></returns>
    [HttpPost("api/workflow/{workflowGuidId:guid}/didcomm")]
    public async Task<ActionResult<string>> DIDCommTriggerEndpoint()
    {
        var hostUrl = string.Concat(_httpContextAccessor!.HttpContext.Request.Scheme, "://", _httpContextAccessor.HttpContext.Request.Host);
        if (System.Diagnostics.Debugger.IsAttached && hostUrl.Equals("http://localhost:5023"))
        {
            // This is only for local development and testing
            hostUrl = "http://host.docker.internal:5023";
        }

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

        // var processMessageResponse = await _mediator.Send(new ProcessMessageRequest(senderOldDid, senderDid, hostUrl, unpacked.Value));

        // return message

        return Ok();
    }
}