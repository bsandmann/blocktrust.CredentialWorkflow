using Blocktrust.CredentialWorkflow.Core.Commands.Outcome.GetOutcomeById;
using Blocktrust.CredentialWorkflow.Core.Commands.Outcome.GetOutcomeIdsByState;
using Blocktrust.CredentialWorkflow.Core.Commands.Outcome.UpdateOutcomeState;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow;
using Blocktrust.CredentialWorkflow.Core.Domain.Enums;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Blocktrust.CredentialWorkflow.Core.Services;

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

        var statesToRescue = new List<EOutcomeState>
        {
            EOutcomeState.NotStarted,
            EOutcomeState.Running
        };

        var rescueResult = await mediator.Send(new GetOutcomeIdsByStateRequest(statesToRescue), cancellationToken);
        if (rescueResult.IsFailed)
        {
            _logger.LogError("Failed to get outcomes on startup: {Errors}", rescueResult.Errors);
        }
        else
        {
            var outcomeIdsToProcess = rescueResult.Value;

            foreach (var outcome in outcomeIdsToProcess)
            {
                if (outcome.OutcomeState == EOutcomeState.Running)
                {
                    var updateStateResult = await mediator.Send(
                        new UpdateOutcomeStateRequest(outcome.OutcomeId, EOutcomeState.NotStarted),
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
                    new UpdateOutcomeStateRequest(outcomeId, EOutcomeState.Running),
                    stoppingToken
                );

                if (updateToRunningResult.IsFailed)
                {
                    _logger.LogError("Failed to set outcome {OutcomeId} to Running: {Errors}",
                        outcomeId, updateToRunningResult.Errors);
                    continue;
                }

                // Get the outcome
                var outcome = await mediator.Send(new GetOutcomeByIdRequest(outcomeId));
                if (outcome.IsFailed)
                {
                    await mediator.Send(
                        new UpdateOutcomeStateRequest(outcomeId, EOutcomeState.FailedWithErrors),
                        stoppingToken
                    );
                    continue;
                }

                // Process the workflow
                var executeResult = await mediator.Send(
                    new ExecuteWorkflowRequest(outcome.Value.Workflow.TenantId, outcome.Value),
                    stoppingToken
                );

                // Update the outcome state
                if (executeResult.IsSuccess)
                {
                    await mediator.Send(
                        new UpdateOutcomeStateRequest(outcomeId, EOutcomeState.Success),
                        stoppingToken
                    );
                }
                else
                {
                    await mediator.Send(
                        new UpdateOutcomeStateRequest(outcomeId, EOutcomeState.FailedWithErrors),
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
                    new UpdateOutcomeStateRequest(outcomeId, EOutcomeState.FailedWithErrors),
                    stoppingToken
                );
            }
        }
    }
}