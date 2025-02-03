using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow;
using Blocktrust.CredentialWorkflow.Core.Domain.Enums;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Blocktrust.CredentialWorkflow.Core.Services;

using Commands.WorkflowOutcome.GetWorkflowOutcomeById;
using Commands.WorkflowOutcome.GetWorkflowOutcomeIdsByState;
using Commands.WorkflowOutcome.UpdateWorkflowOutcome;
using Commands.WorkflowOutcome.UpdateWorkflowOutcomeState;

public class WorkflowProcessingService : BackgroundService
{
    private readonly IWorkflowQueue _workflowQueue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WorkflowProcessingService> _logger;

    public WorkflowProcessingService(
        IWorkflowQueue workflowQueue,
        IServiceProvider serviceProvider,
        ILogger<WorkflowProcessingService> logger)
    {
        _workflowQueue = workflowQueue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting WorkflowProcessingService...");

        using var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var statesToRescue = new List<EWorkflowOutcomeState>
        {
            EWorkflowOutcomeState.NotStarted,
            EWorkflowOutcomeState.Running
        };

        var rescueResult = await mediator.Send(new GetWorkflowOutcomeIdsByStateRequest(statesToRescue), cancellationToken);
        if (rescueResult.IsFailed)
        {
            _logger.LogError("Failed to get outcomes on startup: {Errors}", rescueResult.Errors);
        }
        else
        {
            var outcomeIdsToProcess = rescueResult.Value;

            foreach (var outcome in outcomeIdsToProcess)
            {
                if (outcome.WorkflowOutcomeState == EWorkflowOutcomeState.Running)
                {
                    var updateStateResult = await mediator.Send(
                        new UpdateWorkflowOutcomeStateRequest(outcome.OutcomeId, EWorkflowOutcomeState.NotStarted),
                        cancellationToken
                    );
                    if (updateStateResult.IsFailed)
                    {
                        _logger.LogError("Failed to reset outcome {OutcomeId} to NotStarted: {Errors}",
                            outcome.OutcomeId, updateStateResult.Errors);
                    }
                }

                await _workflowQueue.EnqueueAsync(outcome.OutcomeId, cancellationToken);
            }
        }

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channelReader = ((WorkflowQueue)_workflowQueue).Reader;

        while (!stoppingToken.IsCancellationRequested)
        {
            Guid outcomeId;
            try
            {
                outcomeId = await channelReader.ReadAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Graceful shutdown
                break;
            }

            // Process in a new scope each time we dequeue something
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            try
            {
                _logger.LogInformation("Dequeued Outcome {OutcomeId} for processing", outcomeId);

                // Update to Running
                var updateToRunningResult = await mediator.Send(
                    new UpdateWorkflowOutcomeStateRequest(outcomeId, EWorkflowOutcomeState.Running),
                    stoppingToken
                );

                if (updateToRunningResult.IsFailed)
                {
                    _logger.LogError("Failed to set outcome {OutcomeId} to Running: {Errors}",
                        outcomeId, updateToRunningResult.Errors);
                    continue;
                }

                // Get the outcome
                var outcome = await mediator.Send(new GetWorkflowOutcomeByIdRequest(outcomeId), stoppingToken);
                if (outcome.IsFailed)
                {
                    await mediator.Send(
                        new UpdateWorkflowOutcomeStateRequest(outcomeId, EWorkflowOutcomeState.FailedWithErrors),
                        stoppingToken
                    );
                    continue;
                }

                // Process the workflow
                var executeResult = await mediator.Send(
                    new ExecuteWorkflowRequest(outcome.Value),
                    stoppingToken
                );

                if (executeResult.IsFailed)
                {
                    // This is a critial error
                    await mediator.Send(
                        new UpdateWorkflowOutcomeRequest(outcomeId, EWorkflowOutcomeState.FailedWithErrors, outcomeJson: null, $"Fatal processing error: {executeResult.Errors.FirstOrDefault()?.Message}"),
                        stoppingToken
                    );
                }

                _logger.LogInformation("Processed Outcome {OutcomeId}, success={IsSuccess}",
                    outcomeId, executeResult.IsSuccess);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing Outcome {OutcomeId}", outcomeId);
                await mediator.Send(
                    new UpdateWorkflowOutcomeStateRequest(outcomeId, EWorkflowOutcomeState.FailedWithErrors),
                    stoppingToken
                );
            }
        }
    }
}