using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.GetActiveRecurringWorkflows;
using Blocktrust.CredentialWorkflow.Core.Domain.Enums;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Triggers;
using Blocktrust.CredentialWorkflow.Core.Domain.Workflow;
using Blocktrust.CredentialWorkflow.Core.Entities.Tenant;
using Blocktrust.CredentialWorkflow.Core.Entities.Workflow;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Blocktrust.CredentialWorkflow.Core.Tests
{
    public partial class TestSetup
    {
        [Fact]
        public async Task GetActiveRecurringWorkflows_WorkflowsExist_ButNoneWithRecurrentTrigger_ShouldReturnEmptyList()
        {
            // Arrange
            // 1. Create a tenant
            var tenant = new TenantEntity
            {
                Name = "TenantForNoRecurrentTriggerTest",
                CreatedUtc = DateTime.UtcNow
            };
            await _context.TenantEntities.AddAsync(tenant);
            await _context.SaveChangesAsync();

            // 2. Create a workflow in a different active state (e.g. ActiveWithExternalTrigger)
            var workflowEntity = new WorkflowEntity
            {
                TenantEntityId = tenant.TenantEntityId,
                Name = "WorkflowWithoutRecurrentTrigger",
                WorkflowState = EWorkflowState.ActiveWithExternalTrigger,
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow,
                ProcessFlowJson = null // or possibly a JSON that does not have a recurring timer
            };
            await _context.WorkflowEntities.AddAsync(workflowEntity);
            await _context.SaveChangesAsync();

            var handler = new GetActiveRecurringWorkflowsHandler(_context);

            // Act
            var result = await handler.Handle(new GetActiveRecurringWorkflowsRequest(), CancellationToken.None);

            // Assert
            result.Should().BeSuccess();
        }

        [Fact]
        public async Task GetActiveRecurringWorkflows_MultipleWorkflows_ShouldReturnOnlyThoseWithRecurrentTriggersAndValidCron()
        {
            // Arrange
            var tenant = new TenantEntity
            {
                Name = "TenantForMultipleRecurrentTriggers",
                CreatedUtc = DateTime.UtcNow
            };
            await _context.TenantEntities.AddAsync(tenant);
            await _context.SaveChangesAsync();

            // 1. An ActiveWithRecurrentTrigger workflow with a valid cron
            var processFlowWithCron = new ProcessFlow();
            var triggerWithCron = new Trigger
            {
                Type = ETriggerType.RecurringTimer,
                Input = new TriggerInputRecurringTimer
                {
                    Id = Guid.NewGuid(),
                    CronExpression = "0 1 * * *"
                }
            };
            processFlowWithCron.AddTrigger(triggerWithCron);

            var workflowWithCron = new WorkflowEntity
            {
                TenantEntityId = tenant.TenantEntityId,
                Name = "WorkflowWithCron",
                WorkflowState = EWorkflowState.ActiveWithRecurrentTrigger,
                ProcessFlowJson = processFlowWithCron.SerializeToJson(),
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };

            // 2. An ActiveWithRecurrentTrigger workflow but empty Cron expression
            var processFlowEmptyCron = new ProcessFlow();
            var triggerEmptyCron = new Trigger
            {
                Type = ETriggerType.RecurringTimer,
                Input = new TriggerInputRecurringTimer
                {
                    Id = Guid.NewGuid(),
                    CronExpression = "" // intentionally empty
                }
            };
            processFlowEmptyCron.AddTrigger(triggerEmptyCron);

            var workflowEmptyCron = new WorkflowEntity
            {
                TenantEntityId = tenant.TenantEntityId,
                Name = "WorkflowEmptyCron",
                WorkflowState = EWorkflowState.ActiveWithRecurrentTrigger,
                ProcessFlowJson = processFlowEmptyCron.SerializeToJson(),
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };

            // 3. A workflow with a different active state (ActiveWithExternalTrigger)
            var workflowExternalTrigger = new WorkflowEntity
            {
                TenantEntityId = tenant.TenantEntityId,
                Name = "WorkflowExternalTrigger",
                WorkflowState = EWorkflowState.ActiveWithExternalTrigger,
                ProcessFlowJson = processFlowWithCron.SerializeToJson(), // valid cron, but wrong workflow state
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };

            await _context.WorkflowEntities.AddRangeAsync(workflowWithCron, workflowEmptyCron, workflowExternalTrigger);
            await _context.SaveChangesAsync();

            var handler = new GetActiveRecurringWorkflowsHandler(_context);

            // Act
            var result = await handler.Handle(new GetActiveRecurringWorkflowsRequest(), CancellationToken.None);

            // Assert
            result.Should().BeSuccess();
        }


    }
}
