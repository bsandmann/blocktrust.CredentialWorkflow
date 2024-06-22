namespace Blocktrust.CredentialWorkflow.Core.Tests;

using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.CreateTenant;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.CreateWorkflow;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.GetLatestUpdatedWorkflow;
using Blocktrust.CredentialWorkflow.Core.Entities.Workflow;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using Microsoft.EntityFrameworkCore;

public partial class TestSetup
{
    [Fact]
    public async Task GetLatestUpdatedWorkflow_MultipleWorkflows_ShouldReturnMostRecentlyUpdated()
    {
        // Arrange
        // 1. Create a Tenant
        var createTenantHandler = new CreateTenantHandler(_context);
        var createTenantResult = await createTenantHandler.Handle(new CreateTenantRequest("TestTenant"), CancellationToken.None);
        createTenantResult.Should().BeSuccess();
        var tenantId = createTenantResult.Value;

        // 2. Create multiple Workflows with different update times
        var createWorkflowHandler = new CreateWorkflowHandler(_context);
        await createWorkflowHandler.Handle(new CreateWorkflowRequest(tenantId), CancellationToken.None);
        await createWorkflowHandler.Handle(new CreateWorkflowRequest(tenantId), CancellationToken.None);
        var latestWorkflowResult = await createWorkflowHandler.Handle(new CreateWorkflowRequest(tenantId), CancellationToken.None);
        latestWorkflowResult.Should().BeSuccess();
        var latestWorkflowId = latestWorkflowResult.Value.WorkflowId;

        // Update the last workflow to ensure it's the most recently updated
        var latestWorkflow = await _context.WorkflowEntities.FindAsync(latestWorkflowId);
        latestWorkflow!.UpdatedUtc = DateTime.UtcNow.AddMinutes(1);
        await _context.SaveChangesAsync();

        // 3. Prepare GetLatestUpdatedWorkflow request
        var getLatestUpdatedWorkflowHandler = new GetLatestUpdatedWorkflowHandler(_context);
        var getLatestUpdatedWorkflowRequest = new GetLatestUpdatedWorkflowRequest(tenantId);

        // Act
        var result = await getLatestUpdatedWorkflowHandler.Handle(getLatestUpdatedWorkflowRequest, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().NotBeNull();
        result.Value.WorkflowId.Should().Be(latestWorkflowId);
        result.Value.TenantId.Should().Be(tenantId);
        result.Value.Name.Should().Be("New Workflow");
    }

    [Fact]
    public async Task GetLatestUpdatedWorkflow_NoWorkflows_ShouldFail()
    {
        // Arrange
        // 1. Create a Tenant
        var createTenantHandler = new CreateTenantHandler(_context);
        var createTenantResult = await createTenantHandler.Handle(new CreateTenantRequest("TestTenant"), CancellationToken.None);
        createTenantResult.Should().BeSuccess();
        var tenantId = createTenantResult.Value;

        // 2. Prepare GetLatestUpdatedWorkflow request
        var getLatestUpdatedWorkflowHandler = new GetLatestUpdatedWorkflowHandler(_context);
        var getLatestUpdatedWorkflowRequest = new GetLatestUpdatedWorkflowRequest(tenantId);

        // Act
        var result = await getLatestUpdatedWorkflowHandler.Handle(getLatestUpdatedWorkflowRequest, CancellationToken.None);

        // Assert
        result.Should().BeFailure();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be("No workflows exist in the database.");
    }

    [Fact]
    public async Task GetLatestUpdatedWorkflow_NonExistentTenant_ShouldFail()
    {
        // Arrange
        var nonExistentTenantId = Guid.NewGuid();
        var getLatestUpdatedWorkflowHandler = new GetLatestUpdatedWorkflowHandler(_context);
        var getLatestUpdatedWorkflowRequest = new GetLatestUpdatedWorkflowRequest(nonExistentTenantId);

        // Act
        var result = await getLatestUpdatedWorkflowHandler.Handle(getLatestUpdatedWorkflowRequest, CancellationToken.None);

        // Assert
        result.Should().BeFailure();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be("No workflows exist in the database.");
    }
}