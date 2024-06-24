namespace Blocktrust.CredentialWorkflow.Core.Tests;

using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.CreateTenant;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.CreateWorkflow;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.UpdateWorkflow;
using Blocktrust.CredentialWorkflow.Core.Domain.Enums;
using Blocktrust.CredentialWorkflow.Core.Domain.Workflow;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Domain.ProcessFlow;

public partial class TestSetup
{
    [Fact]
    public async Task UpdateWorkflow_ExistingWorkflow_ShouldSucceed()
    {
        // Arrange
        // 1. Create a Tenant
        var createTenantHandler = new CreateTenantHandler(_context);
        var createTenantResult = await createTenantHandler.Handle(new CreateTenantRequest("TestTenant"), CancellationToken.None);
        createTenantResult.Should().BeSuccess();
        var tenantId = createTenantResult.Value;

        // 2. Create a Workflow
        var createWorkflowHandler = new CreateWorkflowHandler(_context);
        var createWorkflowResult = await createWorkflowHandler.Handle(new CreateWorkflowRequest(tenantId), CancellationToken.None);
        createWorkflowResult.Should().BeSuccess();
        var workflowId = createWorkflowResult.Value.WorkflowId;

        // 3. Prepare UpdateWorkflow request
        var updateWorkflowHandler = new UpdateWorkflowHandler(_context);
        var processFlow = new ProcessFlow { /* Add necessary properties */ };
        var updateWorkflowRequest = new UpdateWorkflowRequest(
            workflowId,
            "Updated Workflow",
            EWorkflowState.ActiveWithExternalTrigger,
            processFlow
        );

        // Act
        var result = await updateWorkflowHandler.Handle(updateWorkflowRequest, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().NotBeNull();
        result.Value.WorkflowId.Should().Be(workflowId);
        result.Value.Name.Should().Be("Updated Workflow");
        result.Value.WorkflowState.Should().Be(EWorkflowState.ActiveWithExternalTrigger);
        result.Value.ProcessFlow.Should().BeEquivalentTo(processFlow);

        // Verify the workflow was actually updated in the database
        var updatedWorkflow = await _context.WorkflowEntities.FindAsync(workflowId);
        updatedWorkflow.Should().NotBeNull();
        updatedWorkflow!.Name.Should().Be("Updated Workflow");
        updatedWorkflow.WorkflowState.Should().Be(EWorkflowState.ActiveWithExternalTrigger);
        var deserializedProcessFlow = ProcessFlow.DeserializeFromJson(updatedWorkflow.ProcessFlowJson);
        deserializedProcessFlow.Should().BeEquivalentTo(processFlow);
    }

    [Fact]
    public async Task UpdateWorkflow_NonExistentWorkflow_ShouldFail()
    {
        // Arrange
        var nonExistentWorkflowId = Guid.NewGuid();
        var updateWorkflowHandler = new UpdateWorkflowHandler(_context);
        var updateWorkflowRequest = new UpdateWorkflowRequest(
            nonExistentWorkflowId,
            "Updated Workflow",
            EWorkflowState.ActiveWithExternalTrigger,
            null
        );

        // Act
        var result = await updateWorkflowHandler.Handle(updateWorkflowRequest, CancellationToken.None);

        // Assert
        result.Should().BeFailure();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be("The workflow does not exist in the database. It cannot be updated.");
    }

    [Fact]
    public async Task UpdateWorkflow_NullProcessFlow_ShouldNotUpdateProcessFlow()
    {
        // Arrange
        // 1. Create a Tenant
        var createTenantHandler = new CreateTenantHandler(_context);
        var createTenantResult = await createTenantHandler.Handle(new CreateTenantRequest("TestTenant"), CancellationToken.None);
        createTenantResult.Should().BeSuccess();
        var tenantId = createTenantResult.Value;

        // 2. Create a Workflow
        var createWorkflowHandler = new CreateWorkflowHandler(_context);
        var createWorkflowResult = await createWorkflowHandler.Handle(new CreateWorkflowRequest(tenantId), CancellationToken.None);
        createWorkflowResult.Should().BeSuccess();
        var workflowId = createWorkflowResult.Value.WorkflowId;

        // 3. Prepare UpdateWorkflow request
        var updateWorkflowHandler = new UpdateWorkflowHandler(_context);
        var updateWorkflowRequest = new UpdateWorkflowRequest(
            workflowId,
            "Updated Workflow",
            EWorkflowState.ActiveWithExternalTrigger,
            null  // Null ProcessFlow
        );

        // Act
        var result = await updateWorkflowHandler.Handle(updateWorkflowRequest, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().NotBeNull();
        result.Value.WorkflowId.Should().Be(workflowId);
        result.Value.Name.Should().Be("Updated Workflow");
        result.Value.WorkflowState.Should().Be(EWorkflowState.ActiveWithExternalTrigger);
        result.Value.ProcessFlow.Should().BeNull();

        // Verify the workflow was actually updated in the database
        var updatedWorkflow = await _context.WorkflowEntities.FindAsync(workflowId);
        updatedWorkflow.Should().NotBeNull();
        updatedWorkflow!.Name.Should().Be("Updated Workflow");
        updatedWorkflow.WorkflowState.Should().Be(EWorkflowState.ActiveWithExternalTrigger);
        updatedWorkflow.ProcessFlowJson.Should().BeNull();
    }
}