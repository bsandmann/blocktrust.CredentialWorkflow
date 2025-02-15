using System.Text.Json;
using Blocktrust.CredentialWorkflow.Core.Commands.WorkflowOutcome.CreateWorkflowOutcome;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.GetWorkflowById;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Triggers;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Blocktrust.CredentialWorkflow.Core.Services;

public interface IFormService
{
    Task<Result<Guid>> ProcessFormSubmission(Guid workflowId, Dictionary<string, string> formData);
}

public class FormService : IFormService
{
    private readonly IMediator _mediator;
    private readonly ILogger<FormService> _logger;
    private readonly IWorkflowQueue _workflowQueue;

    public FormService(IMediator mediator, ILogger<FormService> logger, IWorkflowQueue workflowQueue)
    {
        _mediator = mediator;
        _logger = logger;
        _workflowQueue = workflowQueue;
    }

    public async Task<Result<Guid>> ProcessFormSubmission(Guid workflowId, Dictionary<string, string> formData)
    {
        try
        {
            // Get workflow
            var workflowResult = await _mediator.Send(new GetWorkflowByIdRequest(workflowId));
            if (workflowResult.IsFailed)
            {
                return Result.Fail<Guid>("Workflow not found");
            }

            var workflow = workflowResult.Value;
            var trigger = workflow.ProcessFlow?.Triggers.FirstOrDefault().Value;
                
            if (trigger?.Type != ETriggerType.Form || !(trigger.Input is TriggerInputForm formTrigger))
            {
                return Result.Fail<Guid>("Invalid trigger type");
            }

            // Validate form data against parameters
            foreach (var param in formTrigger.Parameters)
            {
                if (!formData.ContainsKey(param.Key))
                {
                    return Result.Fail<Guid>($"Missing required parameter: {param.Key}");
                }

                // Type validation could be added here
            }

            // Create execution context
            var executionContext = JsonSerializer.Serialize(new
            {
                Type = "FormSubmission",
                Data = formData
            });

            // Create workflow outcome
            var outcomeResult = await _mediator.Send(new CreateWorkflowOutcomeRequest(workflowId, executionContext));
            if (outcomeResult.IsFailed)
            {
                return Result.Fail<Guid>("Failed to create workflow outcome");
            }

            // Enqueue for processing
            await _workflowQueue.EnqueueAsync(outcomeResult.Value);

            return Result.Ok(outcomeResult.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing form submission for workflow {WorkflowId}", workflowId);
            return Result.Fail<Guid>("An error occurred while processing the form submission");
        }
    }
}