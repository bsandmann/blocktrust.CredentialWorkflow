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
using Core.Commands.DIDComm.ProcessMessage;
using DIDComm;
using DIDComm.Model.UnpackParamsModels;

[ApiController]
[AllowAnonymous]
public class DIDCommController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISecretResolver _secretResolver;
    private readonly IDidDocResolver _didDocResolver;

    public DIDCommController(IMediator mediator, IHttpContextAccessor httpContextAccessor, ISecretResolver secretResolver, IDidDocResolver didDocResolver)
    {
        _mediator = mediator;
        _httpContextAccessor = httpContextAccessor;
        _secretResolver = secretResolver;
        _didDocResolver = didDocResolver;
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

        var hostUrl = string.Concat(_httpContextAccessor!.HttpContext.Request.Scheme, "://", _httpContextAccessor.HttpContext.Request.Host);
        // if (System.Diagnostics.Debugger.IsAttached && hostUrl.Equals("http://localhost:5023"))
        // {
        //     // This is only for local development and testing
        //     hostUrl = "http://host.docker.internal:5023";
        // }

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