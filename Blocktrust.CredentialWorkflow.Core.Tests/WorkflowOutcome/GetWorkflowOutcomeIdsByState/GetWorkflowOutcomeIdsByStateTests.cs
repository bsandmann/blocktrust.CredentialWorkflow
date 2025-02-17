using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blocktrust.CredentialWorkflow.Core.Commands.WorkflowOutcome.GetWorkflowOutcomeIdsByState;
using Blocktrust.CredentialWorkflow.Core.Domain.Enums;
using Blocktrust.CredentialWorkflow.Core.Entities.Tenant;
using Blocktrust.CredentialWorkflow.Core.Entities.Workflow;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Blocktrust.CredentialWorkflow.Core.Tests
{
    using Entities.Outcome;

    public partial class TestSetup
    {
        [Fact]
        public async Task GetWorkflowOutcomeIdsByState_EmptyDb_ShouldReturnEmptyList()
        {
            // Arrange
            var request = new GetWorkflowOutcomeIdsByStateRequest(new List<EWorkflowOutcomeState> { EWorkflowOutcomeState.Success });
            var handler = new GetWorkflowOutcomeIdsByStateHandler(_context);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().BeSuccess();
        }

        [Fact]
        public async Task GetWorkflowOutcomeIdsByState_SingleState_ShouldReturnOnlyOutcomesWithThatState()
        {
            // Arrange
            // Create a tenant (optional, but consistent with your DB structure)
            var tenant = new TenantEntity
            {
                TenantEntityId = Guid.NewGuid(),
                Name = "TenantSingleStateTest",
                CreatedUtc = DateTime.UtcNow
            };
            await _context.TenantEntities.AddAsync(tenant);
            await _context.SaveChangesAsync();

            // Create a workflow
            var workflow = new WorkflowEntity
            {
                TenantEntityId = tenant.TenantEntityId,
                Name = "WorkflowForSingleState",
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow,
                WorkflowState = EWorkflowState.ActiveWithExternalTrigger,
                IsRunable = true
            };
            await _context.WorkflowEntities.AddAsync(workflow);
            await _context.SaveChangesAsync();

            // Create multiple outcomes with different states
            var outcomeSuccess = new WorkflowOutcomeEntity
            {
                WorkflowOutcomeEntityId = Guid.NewGuid(),
                WorkflowEntityId = workflow.WorkflowEntityId,
                WorkflowOutcomeState = EWorkflowOutcomeState.Success,
                StartedUtc = DateTime.UtcNow.AddMinutes(-20),
                EndedUtc = DateTime.UtcNow.AddMinutes(-10)
            };
            var outcomeFailed = new WorkflowOutcomeEntity
            {
                WorkflowOutcomeEntityId = Guid.NewGuid(),
                WorkflowEntityId = workflow.WorkflowEntityId,
                WorkflowOutcomeState = EWorkflowOutcomeState.FailedWithErrors,
                StartedUtc = DateTime.UtcNow.AddMinutes(-15),
                EndedUtc = DateTime.UtcNow.AddMinutes(-5)
            };
            await _context.WorkflowOutcomeEntities.AddRangeAsync(outcomeSuccess, outcomeFailed);
            await _context.SaveChangesAsync();

            // We only want outcomes with state = Success
            var request = new GetWorkflowOutcomeIdsByStateRequest(new[] { EWorkflowOutcomeState.Success });
            var handler = new GetWorkflowOutcomeIdsByStateHandler(_context);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().BeSuccess();
        }

        [Fact]
        public async Task GetWorkflowOutcomeIdsByState_MultipleStates_ShouldReturnCorrectOutcomes()
        {
            // Arrange
            var tenant = new TenantEntity
            {
                TenantEntityId = Guid.NewGuid(),
                Name = "TenantMultipleStatesTest",
                CreatedUtc = DateTime.UtcNow
            };
            await _context.TenantEntities.AddAsync(tenant);
            await _context.SaveChangesAsync();

            var workflow = new WorkflowEntity
            {
                TenantEntityId = tenant.TenantEntityId,
                Name = "WorkflowForMultipleStates",
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow,
                WorkflowState = EWorkflowState.Inactive,
                IsRunable = false
            };
            await _context.WorkflowEntities.AddAsync(workflow);
            await _context.SaveChangesAsync();

            var outcomeSuccess = new WorkflowOutcomeEntity
            {
                WorkflowOutcomeEntityId = Guid.NewGuid(),
                WorkflowEntityId = workflow.WorkflowEntityId,
                WorkflowOutcomeState = EWorkflowOutcomeState.Success,
                StartedUtc = DateTime.UtcNow.AddMinutes(-50),
                EndedUtc = DateTime.UtcNow.AddMinutes(-49)
            };
            var outcomeFail = new WorkflowOutcomeEntity
            {
                WorkflowOutcomeEntityId = Guid.NewGuid(),
                WorkflowEntityId = workflow.WorkflowEntityId,
                WorkflowOutcomeState = EWorkflowOutcomeState.FailedWithErrors,
                StartedUtc = DateTime.UtcNow.AddMinutes(-40),
                EndedUtc = DateTime.UtcNow.AddMinutes(-39)
            };
            var outcomeRunning = new WorkflowOutcomeEntity
            {
                WorkflowOutcomeEntityId = Guid.NewGuid(),
                WorkflowEntityId = workflow.WorkflowEntityId,
                WorkflowOutcomeState = EWorkflowOutcomeState.Running,
                StartedUtc = DateTime.UtcNow.AddMinutes(-10),
                EndedUtc = null
            };

            await _context.WorkflowOutcomeEntities.AddRangeAsync(outcomeSuccess, outcomeFail, outcomeRunning);
            await _context.SaveChangesAsync();

            var requestedStates = new[] { EWorkflowOutcomeState.Success, EWorkflowOutcomeState.Running };
            var request = new GetWorkflowOutcomeIdsByStateRequest(requestedStates);
            var handler = new GetWorkflowOutcomeIdsByStateHandler(_context);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().BeSuccess();
        }

        [Fact]
        public async Task GetWorkflowOutcomeIdsByState_NoMatches_ShouldReturnEmptyList()
        {
            // Arrange
            // Create one outcome in "Success" but we'll request "FailedWithErrors"
            var tenant = new TenantEntity
            {
                TenantEntityId = Guid.NewGuid(),
                Name = "TenantNoMatchTest",
                CreatedUtc = DateTime.UtcNow
            };
            await _context.TenantEntities.AddAsync(tenant);
            await _context.SaveChangesAsync();

            var workflow = new WorkflowEntity
            {
                TenantEntityId = tenant.TenantEntityId,
                Name = "WorkflowNoMatch",
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow,
                WorkflowState = EWorkflowState.ActiveWithExternalTrigger,
                IsRunable = true
            };
            await _context.WorkflowEntities.AddAsync(workflow);
            await _context.SaveChangesAsync();

            var outcomeSuccess = new WorkflowOutcomeEntity
            {
                WorkflowOutcomeEntityId = Guid.NewGuid(),
                WorkflowEntityId = workflow.WorkflowEntityId,
                WorkflowOutcomeState = EWorkflowOutcomeState.Success,
                StartedUtc = DateTime.UtcNow.AddMinutes(-20),
                EndedUtc = DateTime.UtcNow.AddMinutes(-10)
            };
            await _context.WorkflowOutcomeEntities.AddAsync(outcomeSuccess);
            await _context.SaveChangesAsync();

            // We're requesting 'FailedWithErrors', but the DB only has 'Success'.
            var request = new GetWorkflowOutcomeIdsByStateRequest(new[] { EWorkflowOutcomeState.FailedWithErrors });
            var handler = new GetWorkflowOutcomeIdsByStateHandler(_context);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().BeSuccess();
        }

        [Fact]
        public async Task GetWorkflowOutcomeIdsByState_EmptyStatesList_ShouldReturnEmptyList()
        {
            // Arrange
            // Add a random outcome anyway; since we won't specify any states, we shouldn't get anything back.
            var tenant = new TenantEntity
            {
                TenantEntityId = Guid.NewGuid(),
                Name = "TenantEmptyStateListTest",
                CreatedUtc = DateTime.UtcNow
            };
            await _context.TenantEntities.AddAsync(tenant);
            await _context.SaveChangesAsync();

            var workflow = new WorkflowEntity
            {
                TenantEntityId = tenant.TenantEntityId,
                Name = "WorkflowEmptyStateList",
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow,
                WorkflowState = EWorkflowState.ActiveWithExternalTrigger,
                IsRunable = true
            };
            await _context.WorkflowEntities.AddAsync(workflow);
            await _context.SaveChangesAsync();

            var outcomeRunning = new WorkflowOutcomeEntity
            {
                WorkflowOutcomeEntityId = Guid.NewGuid(),
                WorkflowEntityId = workflow.WorkflowEntityId,
                WorkflowOutcomeState = EWorkflowOutcomeState.Running,
                StartedUtc = DateTime.UtcNow.AddMinutes(-10),
                EndedUtc = null
            };
            await _context.WorkflowOutcomeEntities.AddAsync(outcomeRunning);
            await _context.SaveChangesAsync();

            var request = new GetWorkflowOutcomeIdsByStateRequest(Array.Empty<EWorkflowOutcomeState>());
            var handler = new GetWorkflowOutcomeIdsByStateHandler(_context);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().BeSuccess();
            result.Value.Should().BeEmpty();
        }
    }
}
