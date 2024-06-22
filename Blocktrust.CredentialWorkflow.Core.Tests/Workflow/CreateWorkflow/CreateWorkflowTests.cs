namespace Blocktrust.CredentialWorkflow.Core.Tests;

using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.CreateWorkflow;
using Blocktrust.CredentialWorkflow.Core.Domain.Enums;
using Blocktrust.CredentialWorkflow.Core.Entities.Tenant;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using Microsoft.EntityFrameworkCore;

public partial class TestSetup
{
    [Fact]
    public async Task CreateWorkflow_ForExistingTenant_ShouldSucceed()
    {
        // Arrange
        var tenant = new TenantEntity
        {
            Name = "TestTenant",
            CreatedUtc = DateTime.UtcNow
        };
        await _context.TenantEntities.AddAsync(tenant);
        await _context.SaveChangesAsync();

        var request = new CreateWorkflowRequest(tenant.TenantEntityId);
        var handler = new CreateWorkflowHandler(_context);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().NotBeNull();
        result.Value.TenantId.Should().Be(tenant.TenantEntityId);
        result.Value.Name.Should().Be("New Workflow");
        result.Value.WorkflowState.Should().Be(EWorkflowState.Inactive);
        result.Value.ProcessFlow.Should().BeNull();

        // Verify the workflow was actually added to the database
        var workflowInDb = await _context.WorkflowEntities
            .FirstOrDefaultAsync(w => w.TenantEntityId == tenant.TenantEntityId);
        workflowInDb.Should().NotBeNull();
        workflowInDb!.Name.Should().Be("New Workflow");
        workflowInDb.WorkflowState.Should().Be(EWorkflowState.Inactive);
        workflowInDb.ProcessFlowJson.Should().BeNull();
    }
    
    [Fact]
    public async Task CreateMultipleWorkflows_ForSameTenant_ShouldSucceed()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenant = new TenantEntity
        {
            TenantEntityId = tenantId,
            Name = "TestTenant",
            CreatedUtc = DateTime.UtcNow
        };
        await _context.TenantEntities.AddAsync(tenant);
        await _context.SaveChangesAsync();

        var request = new CreateWorkflowRequest(tenantId);
        var handler = new CreateWorkflowHandler(_context);

        // Act
        var result1 = await handler.Handle(request, CancellationToken.None);
        var result2 = await handler.Handle(request, CancellationToken.None);
        var result3 = await handler.Handle(request, CancellationToken.None);

        // Assert
        result1.Should().BeSuccess();
        result2.Should().BeSuccess();
        result3.Should().BeSuccess();

        result1.Value.Should().NotBeNull();
        result2.Value.Should().NotBeNull();
        result3.Value.Should().NotBeNull();

        result1.Value.WorkflowId.Should().NotBe(result2.Value.WorkflowId);
        result2.Value.WorkflowId.Should().NotBe(result3.Value.WorkflowId);
        result1.Value.WorkflowId.Should().NotBe(result3.Value.WorkflowId);

        // Verify the workflows were actually added to the database
        var workflowsInDb = await _context.WorkflowEntities
            .Where(w => w.TenantEntityId == tenantId)
            .ToListAsync();

        workflowsInDb.Should().HaveCount(3);
        workflowsInDb.Select(w => w.WorkflowEntityId).Should().OnlyHaveUniqueItems();
        workflowsInDb.Should().AllSatisfy(w =>
        {
            w.Name.Should().Be("New Workflow");
            w.WorkflowState.Should().Be(EWorkflowState.Inactive);
            w.ProcessFlowJson.Should().BeNull();
            w.TenantEntityId.Should().Be(tenantId);
        });
    } 
    
}