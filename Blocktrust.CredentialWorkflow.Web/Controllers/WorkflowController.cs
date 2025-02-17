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

[ApiController]
[Route("api/workflow/{workflowGuidId:guid}")]
[AllowAnonymous]
public class WorkflowController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITriggerValidationService _triggerValidationService;
    private readonly IWorkflowQueue _workflowQueue;

    public WorkflowController(IMediator mediator, ITriggerValidationService triggerValidationService, IWorkflowQueue workflowQueue)
    {
        _mediator = mediator;
        _triggerValidationService = triggerValidationService;
        _workflowQueue = workflowQueue;
    }

    [HttpGet]
    public Task<IActionResult> ExecuteWorkflowGet([FromRoute] Guid workflowGuidId)
    {
        return ExecuteWorkflowInternalAsync(HttpRequestMethod.GET, workflowGuidId);
    }

    [HttpPost]
    public Task<IActionResult> ExecuteWorkflowPost([FromRoute] Guid workflowGuidId)
    {
        return ExecuteWorkflowInternalAsync(HttpRequestMethod.POST, workflowGuidId);
    }

    private async Task<IActionResult> ExecuteWorkflowInternalAsync(HttpRequestMethod httpMethod, Guid workflowGuidId)
    {
        // Create and populate a new instance of SimplifiedHttpContext
        var simplifiedHttpContext = new SimplifiedHttpContext
        {
            Method = httpMethod
        };

        // Populate query parameters
        foreach (var queryParam in Request.Query)
        {
            simplifiedHttpContext.QueryParameters[queryParam.Key] = queryParam.Value.ToString();
        }

        // Read the entire request body (if any)
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
        {
            simplifiedHttpContext.Body = await reader.ReadToEndAsync();
        }

        // Retrieve the workflow details via MediatR
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

        // Ensure that the trigger is an HTTP request trigger
        var trigger = processFlow.Triggers.First().Value;
        if (trigger.Type != ETriggerType.HttpRequest)
        {
            return BadRequest("The workflow trigger is not an HTTP request trigger");
        }

        // Validate the HTTP trigger using the simplified context
        var httpRequestTrigger = (TriggerInputHttpRequest)trigger.Input;
        var validationResult = _triggerValidationService.Validate(httpRequestTrigger, simplifiedHttpContext);
        if (validationResult.IsFailed)
        {
            return BadRequest(validationResult.Errors.First().Message);
        }

        var executionContext = simplifiedHttpContext.ToJson();

        var outcomeResult = await _mediator.Send(new CreateWorkflowOutcomeRequest(getWorkflowResult.Value.WorkflowId, executionContext));
        if (outcomeResult.IsFailed)
        {
            return BadRequest($"Failed to create outcome for workflow {getWorkflowResult.Value.WorkflowId}");
        }

        await _workflowQueue.EnqueueAsync(outcomeResult.Value);

        return Ok("Workflow retrieved successfully");
    }
}