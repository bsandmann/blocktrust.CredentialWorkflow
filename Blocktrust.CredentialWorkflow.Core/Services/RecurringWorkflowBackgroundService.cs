namespace Blocktrust.CredentialWorkflow.Core.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Blocktrust.CredentialWorkflow.Core.Commands.Outcome.CreateOutcome;
    using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.GetActiveRecurringWorkflows;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using NCrontab;

    public class RecurringWorkflowBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RecurringWorkflowBackgroundService> _logger;
        private const int CheckIntervalSeconds = 30;
        private readonly IWorkflowQueue _workflowQueue;


        public RecurringWorkflowBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<RecurringWorkflowBackgroundService> logger,
            IWorkflowQueue workflowQueue)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _workflowQueue = workflowQueue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RecurringWorkflowBackgroundService started.");

            // Run loop until the host is stopped.
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Create a new scope so that you can use scoped services (like EF Core DbContext, etc.)
                    using var scope = _serviceProvider.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                    // Retrieve active recurring workflows.
                    var workflowsResult = await mediator.Send(new GetActiveRecurringWorkflowsRequest(), stoppingToken);
                    if (workflowsResult.IsSuccess)
                    {
                        var workflows = workflowsResult.Value;
                        var now = DateTime.UtcNow;
                        var windowStart = now - TimeSpan.FromSeconds(CheckIntervalSeconds);

                        foreach (var workflow in workflows)
                        {
                            // Skip if there is no cron expression.
                            if (string.IsNullOrWhiteSpace(workflow.CronExpression))
                            {
                                _logger.LogWarning("Workflow {WorkflowId} does not have a valid cron expression.", workflow.WorkflowEntityId);
                                continue;
                            }

                            CrontabSchedule schedule;
                            try
                            {
                                schedule = CrontabSchedule.Parse(workflow.CronExpression);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Invalid cron expression for workflow {WorkflowId}: {CronExpression}",
                                    workflow.WorkflowEntityId, workflow.CronExpression);
                                continue;
                            }

                            // Check if there is an occurrence scheduled in the past 30 seconds.
                            // This works by getting the next occurrence from (now - 30 seconds)
                            // and checking if that occurrence is between (now - 30 seconds) and now.
                            var nextOccurrence = schedule.GetNextOccurrence(windowStart);
                            if (nextOccurrence > windowStart && nextOccurrence <= now)
                            {
                                _logger.LogInformation("Workflow {WorkflowId} is due to run (scheduled at {Occurrence}). Triggering CreateOutcome.",
                                    workflow.WorkflowEntityId, nextOccurrence);

                                // Call the CreateOutcome command.
                                var outcomeResult = await mediator.Send(new CreateOutcomeRequest(workflow.WorkflowEntityId, null), stoppingToken);
                                if (outcomeResult.IsFailed)
                                {
                                    _logger.LogError("Failed to create outcome for workflow {WorkflowId}.", workflow.WorkflowEntityId);
                                }
                                await _workflowQueue.EnqueueAsync(outcomeResult.Value, stoppingToken);
                            }
                            else
                            {
                                _logger.LogDebug("Workflow {WorkflowId} is not scheduled to run at this time. Next occurrence: {Occurrence}.",
                                    workflow.WorkflowEntityId, nextOccurrence);
                            }
                        }
                    }
                    else
                    {
                        _logger.LogError("Failed to retrieve active recurring workflows.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while checking recurring workflows.");
                }

                // Wait for 30 seconds (or until cancellation is requested) before the next check.
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(CheckIntervalSeconds), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // The delay was canceled because the host is shutting down.
                }
            }

            _logger.LogInformation("RecurringWorkflowBackgroundService is stopping.");
        }
    }
}
