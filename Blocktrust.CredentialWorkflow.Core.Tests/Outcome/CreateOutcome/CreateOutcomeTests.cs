namespace Blocktrust.CredentialWorkflow.Core.Tests;

using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.CreateTenant;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.CreateWorkflow;
using Blocktrust.CredentialWorkflow.Core.Commands.Outcome.CreateOutcome;
using Blocktrust.CredentialWorkflow.Core.Domain.Enums;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using Microsoft.EntityFrameworkCore;

public partial class TestSetup
{
    [Fact]
    public async Task CreateOutcome_ExistingWorkflow_ShouldSucceed()
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

        // 3. Prepare CreateOutcome request
        var createOutcomeHandler = new CreateOutcomeHandler(_context);
        var createOutcomeRequest = new CreateOutcomeRequest(workflowId);

        // Act
        var result = await createOutcomeHandler.Handle(createOutcomeRequest, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().NotBe(Guid.Empty);

        // Verify the outcome was actually created in the database
        var createdOutcome = await _context.OutcomeEntities.FindAsync(result.Value);
        createdOutcome.Should().NotBeNull();
        createdOutcome!.WorkflowEntityId.Should().Be(workflowId);
        createdOutcome.OutcomeState.Should().Be(EOutcomeState.NotStarted);
        createdOutcome.StartedUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task CreateOutcome_NonExistentWorkflow_ShouldFail()
    {
        // Arrange
        var nonExistentWorkflowId = Guid.NewGuid();
        var createOutcomeHandler = new CreateOutcomeHandler(_context);
        var createOutcomeRequest = new CreateOutcomeRequest(nonExistentWorkflowId);

        // Act
        var result = await createOutcomeHandler.Handle(createOutcomeRequest, CancellationToken.None);

        // Assert
        result.Should().BeFailure();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be("The workflow does not exist in the database. The outcome cannot be created.");
    }

    [Fact]
    public async Task CreateOutcome_MultipleOutcomesForSameWorkflow_ShouldSucceed()
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

        // 3. Prepare CreateOutcome handler
        var createOutcomeHandler = new CreateOutcomeHandler(_context);
        var createOutcomeRequest = new CreateOutcomeRequest(workflowId);

        // Act
        var result1 = await createOutcomeHandler.Handle(createOutcomeRequest, CancellationToken.None);
        var result2 = await createOutcomeHandler.Handle(createOutcomeRequest, CancellationToken.None);

        // Assert
        result1.Should().BeSuccess();
        result2.Should().BeSuccess();
        result1.Value.Should().NotBe(result2.Value);

        // Verify both outcomes were actually created in the database
        var createdOutcomes = await _context.OutcomeEntities.Where(o => o.WorkflowEntityId == workflowId).ToListAsync();
        createdOutcomes.Should().HaveCount(2);
        createdOutcomes.Should().AllSatisfy(o => 
        {
            o.WorkflowEntityId.Should().Be(workflowId);
            o.OutcomeState.Should().Be(EOutcomeState.NotStarted);
            o.StartedUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        });
    }
}