namespace Blocktrust.CredentialWorkflow.Core.Tests;

using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.DeleteWorkflow;
using Blocktrust.CredentialWorkflow.Core.Entities.Workflow;
using Blocktrust.CredentialWorkflow.Core.Domain.Enums;
using Entities.Tenant;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using Microsoft.EntityFrameworkCore;

public partial class TestSetup
{
    [Fact]
    public async Task DeleteWorkflow_ExistingWorkflow_ShouldSucceed()
    {
        // Arrange
        var tenant = new TenantEntity
        {
            Name = "TestTenant",
            CreatedUtc = DateTime.UtcNow
        };
        
        await _context.TenantEntities.AddAsync(tenant);
        await _context.SaveChangesAsync();
        
        var workflow = new WorkflowEntity
        {
            Name = "Test Workflow",
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
            TenantEntityId = tenant.TenantEntityId,
            WorkflowState = EWorkflowState.Inactive,
            ProcessFlowJson = null
        };
        await _context.WorkflowEntities.AddAsync(workflow);
        await _context.SaveChangesAsync();

        var request = new DeleteWorkflowRequest(workflow.WorkflowEntityId);
        var handler = new DeleteWorkflowHandler(_context);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();

        // Verify the workflow was actually deleted from the database
        var deletedWorkflow = await _context.WorkflowEntities.FindAsync(workflow.WorkflowEntityId);
        deletedWorkflow.Should().BeNull();
    }

    [Fact]
    public async Task DeleteWorkflow_NonExistentWorkflow_ShouldFail()
    {
        // Arrange
        var nonExistentWorkflowId = Guid.NewGuid();
        var request = new DeleteWorkflowRequest(nonExistentWorkflowId);
        var handler = new DeleteWorkflowHandler(_context);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeFailure();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be("The workflow does not exist in the database. It cannot be deleted.");
    }
}