using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ChangeWorkflowState;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.CreateWorkflow;
using Blocktrust.CredentialWorkflow.Core.Domain.Enums;
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
        public async Task ChangeWorkflowState_ForExistingWorkflow_ShouldSucceed()
        {
            // Arrange
            var tenant = new TenantEntity
            {
                Name = "TestTenant-ChangeState",
                CreatedUtc = DateTime.UtcNow
            };
            await _context.TenantEntities.AddAsync(tenant);
            await _context.SaveChangesAsync();

            // Create a workflow in the Inactive state
            var createRequest = new CreateWorkflowRequest(tenant.TenantEntityId, "Test Workflow", null);
            var createHandler = new CreateWorkflowHandler(_context);
            var createResult = await createHandler.Handle(createRequest, CancellationToken.None);

            createResult.Should().BeSuccess();
            var createdWorkflow = createResult.Value;
            createdWorkflow.WorkflowState.Should().Be(EWorkflowState.Inactive);

            // Act
            // Now change the workflow state from Inactive to Active
            var changeRequest = new ChangeWorkflowStateRequest(createdWorkflow.WorkflowId, EWorkflowState.ActiveWithExternalTrigger);
            var changeHandler = new ChangeWorkflowStateHandler(_context);
            var changeResult = await changeHandler.Handle(changeRequest, CancellationToken.None);

            // Assert
            changeResult.Should().BeSuccess();
            changeResult.Value.WorkflowState.Should().Be(EWorkflowState.ActiveWithExternalTrigger);

            // Verify that the workflow was updated in the database
            var workflowInDb = await _context.WorkflowEntities
                .FirstOrDefaultAsync(w => w.WorkflowEntityId == createdWorkflow.WorkflowId);
            workflowInDb.Should().NotBeNull();
            workflowInDb!.WorkflowState.Should().Be(EWorkflowState.ActiveWithExternalTrigger);
            workflowInDb.IsRunable.Should().BeTrue(); // Because it's no longer Inactive
        }

        [Fact]
        public async Task ChangeWorkflowState_ForMultipleWorkflows_ShouldSucceed()
        {
            // Arrange
            var tenant = new TenantEntity
            {
                Name = "TestTenant-MultipleChanges",
                CreatedUtc = DateTime.UtcNow
            };
            await _context.TenantEntities.AddAsync(tenant);
            await _context.SaveChangesAsync();

            // Create multiple workflows
            var createHandler = new CreateWorkflowHandler(_context);

            var request1 = new CreateWorkflowRequest(tenant.TenantEntityId, "Workflow1", null);
            var request2 = new CreateWorkflowRequest(tenant.TenantEntityId, "Workflow2", null);
            var request3 = new CreateWorkflowRequest(tenant.TenantEntityId, "Workflow3", null);

            var result1 = await createHandler.Handle(request1, CancellationToken.None);
            var result2 = await createHandler.Handle(request2, CancellationToken.None);
            var result3 = await createHandler.Handle(request3, CancellationToken.None);

            result1.Should().BeSuccess();
            result2.Should().BeSuccess();
            result3.Should().BeSuccess();

            // Act
            // Change all of them from Inactive to Disabled
            var changeHandler = new ChangeWorkflowStateHandler(_context);
            var changeResult1 = await changeHandler.Handle(new ChangeWorkflowStateRequest(result1.Value.WorkflowId, EWorkflowState.ActiveWithExternalTrigger), CancellationToken.None);
            var changeResult2 = await changeHandler.Handle(new ChangeWorkflowStateRequest(result2.Value.WorkflowId, EWorkflowState.ActiveWithExternalTrigger), CancellationToken.None);
            var changeResult3 = await changeHandler.Handle(new ChangeWorkflowStateRequest(result3.Value.WorkflowId, EWorkflowState.ActiveWithExternalTrigger), CancellationToken.None);

            // Assert
            changeResult1.Should().BeSuccess();
            changeResult2.Should().BeSuccess();
            changeResult3.Should().BeSuccess();

            // Verify updates in the database
            var workflowIds = new[] { result1.Value.WorkflowId, result2.Value.WorkflowId, result3.Value.WorkflowId };
            var workflowsInDb = await _context.WorkflowEntities
                .Where(w => workflowIds.Contains(w.WorkflowEntityId))
                .ToListAsync();

            workflowsInDb.Should().HaveCount(3);
            workflowsInDb.Should().AllSatisfy(w =>
            {
                w.WorkflowState.Should().Be(EWorkflowState.ActiveWithExternalTrigger);
                w.IsRunable.Should().BeTrue(); // Because Disabled != Inactive
            });
        }

        [Fact]
        public async Task ChangeWorkflowState_ForNonExistentWorkflow_ShouldFail()
        {
            // Arrange
            var nonExistentWorkflowId = Guid.NewGuid();
            var changeHandler = new ChangeWorkflowStateHandler(_context);

            // Act
            var changeRequest = new ChangeWorkflowStateRequest(nonExistentWorkflowId, EWorkflowState.ActiveWithExternalTrigger);
            var changeResult = await changeHandler.Handle(changeRequest, CancellationToken.None);

            // Assert
            changeResult.Should().BeFailure();
            changeResult.Errors.Should().ContainSingle(e => e.Message
                .Contains("The workflow does not exist in the database"));
        }
    }
}
