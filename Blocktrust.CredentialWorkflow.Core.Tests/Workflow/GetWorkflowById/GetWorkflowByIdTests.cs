namespace Blocktrust.CredentialWorkflow.Core.Tests;

using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.CreateTenant;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.CreateWorkflow;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.GetWorkflowById;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using Microsoft.EntityFrameworkCore;

public partial class TestSetup
{
    [Fact]
    public async Task GetWorkflowById_ExistingWorkflow_ShouldSucceed()
    {
        // Arrange
        // 1. Create a Tenant
        var createTenantHandler = new CreateTenantHandler(_context);
        var createTenantRequest = new CreateTenantRequest("TestTenant");
        var createTenantResult = await createTenantHandler.Handle(createTenantRequest, CancellationToken.None);
        createTenantResult.Should().BeSuccess();
        var tenantId = createTenantResult.Value;

        // 2. Create a Workflow
        var createWorkflowHandler = new CreateWorkflowHandler(_context);
        var createWorkflowRequest = new CreateWorkflowRequest(tenantId);
        var createWorkflowResult = await createWorkflowHandler.Handle(createWorkflowRequest, CancellationToken.None);
        createWorkflowResult.Should().BeSuccess();
        var workflowId = createWorkflowResult.Value.WorkflowId;

        // 3. Prepare GetWorkflowById request
        var getWorkflowByIdHandler = new GetWorkflowByIdHandler(_context);
        var getWorkflowByIdRequest = new GetWorkflowByIdRequest(workflowId);

        // Act
        var result = await getWorkflowByIdHandler.Handle(getWorkflowByIdRequest, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().NotBeNull();
        result.Value.WorkflowId.Should().Be(workflowId);
        result.Value.TenantId.Should().Be(tenantId);
        result.Value.Name.Should().Be("New Workflow");
        result.Value.WorkflowState.Should().Be(Domain.Enums.EWorkflowState.Inactive);
        result.Value.ProcessFlow.Should().BeNull();

        // Verify the workflow exists in the database
        var workflowInDb = await _context.WorkflowEntities.FindAsync(workflowId);
        workflowInDb.Should().NotBeNull();
        workflowInDb!.WorkflowEntityId.Should().Be(workflowId);
        workflowInDb.TenantEntityId.Should().Be(tenantId);
    }

    [Fact]
    public async Task GetWorkflowById_NonExistentWorkflow_ShouldFail()
    {
        // Arrange
        var nonExistentWorkflowId = Guid.NewGuid();
        var getWorkflowByIdHandler = new GetWorkflowByIdHandler(_context);
        var getWorkflowByIdRequest = new GetWorkflowByIdRequest(nonExistentWorkflowId);

        // Act
        var result = await getWorkflowByIdHandler.Handle(getWorkflowByIdRequest, CancellationToken.None);

        // Assert
        result.Should().BeFailure();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be("The workflow does not exist in the database.");
    }
}