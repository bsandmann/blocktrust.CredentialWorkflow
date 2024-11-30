using Blocktrust.CredentialWorkflow.Core.Commands.Outcome.CreateOutcome;
using Blocktrust.CredentialWorkflow.Core.Commands.Outcome.GetOutcomeById;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Triggers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Blocktrust.CredentialWorkflow.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkflowExecutionController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<WorkflowExecutionController> _logger;

    public WorkflowExecutionController(
        IMediator mediator,
        ILogger<WorkflowExecutionController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("credentials/issue/{workflowId}")]
    public async Task<IActionResult> ExecuteCredentialIssuance(
        Guid workflowId,
        [FromBody] TriggerInputCredentialIssuance input,
        CancellationToken cancellationToken)
    {
        try
        {
            var outcomeResult = await _mediator.Send(new CreateOutcomeRequest(workflowId), cancellationToken);
            if (outcomeResult.IsFailed)
            {
                return BadRequest(outcomeResult.Errors);
            }

            // TODO: Send workflow execution command via mediator
            // The workflow engine will:
            // 1. Load workflow definition
            // 2. Execute actions (issuance + delivery)
            // 3. Handle results and errors

            return Ok(new { outcomeId = outcomeResult.Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute workflow {WorkflowId}", workflowId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("outcomes/{outcomeId}")]
    public async Task<IActionResult> GetOutcome(
        Guid outcomeId,
        CancellationToken cancellationToken)
    {
        try
        {
            var outcome = await _mediator.Send(new GetOutcomeByIdRequest(outcomeId), cancellationToken);
            if (outcome.IsFailed)
            {
                return NotFound(outcome.Errors);
            }

            return Ok(outcome.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get outcome {OutcomeId}", outcomeId);
            return StatusCode(500, "Internal server error");
        }
    }
}