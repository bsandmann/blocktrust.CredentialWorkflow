using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.CreateTenant;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.CreateWorkflow;
using Blocktrust.CredentialWorkflow.Core.Commands.Outcome.CreateOutcome;
using Blocktrust.CredentialWorkflow.Core.Commands.Outcome.GetOutcomes;
using Blocktrust.CredentialWorkflow.Core.Domain.Enums;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Blocktrust.CredentialWorkflow.Core.Tests;

public partial class TestSetup
{
    [Fact]
    public async Task GetOutcomes_ExistingWorkflowWithOutcomes_ShouldSucceed()
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

        // 3. Create multiple WorkflowOutcome
        var createOutcomeHandler = new CreateOutcomeHandler(_context);
        var expectedOutcomeIds = new List<Guid>();
        for (int i = 0; i < 3; i++)
        {
            var createOutcomeResult = await createOutcomeHandler.Handle(new CreateOutcomeRequest(workflowId), CancellationToken.None);
            createOutcomeResult.Should().BeSuccess();
            expectedOutcomeIds.Add(createOutcomeResult.Value);
        }

        // 4. Prepare GetOutcomes request
        var getOutcomesHandler = new GetOutcomesHandler(_context);
        var getOutcomesRequest = new GetOutcomesRequest(workflowId);

        // Act
        var result = await getOutcomesHandler.Handle(getOutcomesRequest, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(3);
        result.Value.Select(o => o.OutcomeId).Should().BeEquivalentTo(expectedOutcomeIds);
        result.Value.Should().AllSatisfy(o =>
        {
            o.WorkflowId.Should().Be(workflowId);
            o.OutcomeState.Should().Be(EOutcomeState.NotStarted);
            o.StartedUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        });
    }

    [Fact]
    public async Task GetOutcomes_ExistingWorkflowWithNoOutcomes_ShouldReturnEmptyList()
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

        // 3. Prepare GetOutcomes request
        var getOutcomesHandler = new GetOutcomesHandler(_context);
        var getOutcomesRequest = new GetOutcomesRequest(workflowId);

        // Act
        var result = await getOutcomesHandler.Handle(getOutcomesRequest, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetOutcomes_NonExistentWorkflow_ShouldReturnEmptyList()
    {
        // Arrange
        var nonExistentWorkflowId = Guid.NewGuid();
        var getOutcomesHandler = new GetOutcomesHandler(_context);
        var getOutcomesRequest = new GetOutcomesRequest(nonExistentWorkflowId);

        // Act
        var result = await getOutcomesHandler.Handle(getOutcomesRequest, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetOutcomes_MultipleWorkflows_ShouldReturnCorrectOutcomes()
    {
        // Arrange
        // 1. Create a Tenant
        var createTenantHandler = new CreateTenantHandler(_context);
        var createTenantResult = await createTenantHandler.Handle(new CreateTenantRequest("TestTenant"), CancellationToken.None);
        createTenantResult.Should().BeSuccess();
        var tenantId = createTenantResult.Value;

        // 2. Create multiple Workflows
        var createWorkflowHandler = new CreateWorkflowHandler(_context);
        var workflowIds = new List<Guid>();
        for (int i = 0; i < 2; i++)
        {
            var createWorkflowResult = await createWorkflowHandler.Handle(new CreateWorkflowRequest(tenantId), CancellationToken.None);
            createWorkflowResult.Should().BeSuccess();
            workflowIds.Add(createWorkflowResult.Value.WorkflowId);
        }

        // 3. Create WorkflowOutcome for each Workflow
        var createOutcomeHandler = new CreateOutcomeHandler(_context);
        var expectedOutcomeIds = new List<Guid>();
        foreach (var workflowId in workflowIds)
        {
            for (int i = 0; i < 2; i++)
            {
                var createOutcomeResult = await createOutcomeHandler.Handle(new CreateOutcomeRequest(workflowId), CancellationToken.None);
                createOutcomeResult.Should().BeSuccess();
                if (workflowId == workflowIds[0])
                {
                    expectedOutcomeIds.Add(createOutcomeResult.Value);
                }
            }
        }

        // 4. Prepare GetOutcomes request for the first workflow
        var getOutcomesHandler = new GetOutcomesHandler(_context);
        var getOutcomesRequest = new GetOutcomesRequest(workflowIds[0]);

        // Act
        var result = await getOutcomesHandler.Handle(getOutcomesRequest, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.Select(o => o.OutcomeId).Should().BeEquivalentTo(expectedOutcomeIds);
        result.Value.Should().AllSatisfy(o =>
        {
            o.WorkflowId.Should().Be(workflowIds[0]);
            o.OutcomeState.Should().Be(EOutcomeState.NotStarted);
            o.StartedUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        });
    }
}