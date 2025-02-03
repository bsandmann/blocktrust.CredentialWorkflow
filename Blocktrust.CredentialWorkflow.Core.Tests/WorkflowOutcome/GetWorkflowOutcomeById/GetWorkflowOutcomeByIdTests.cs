using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.CreateTenant;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.CreateWorkflow;
using Blocktrust.CredentialWorkflow.Core.Domain.Enums;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Blocktrust.CredentialWorkflow.Core.Tests;

using Core.Commands.WorkflowOutcome.CreateWorkflowOutcome;
using Core.Commands.WorkflowOutcome.GetWorkflowOutcomeById;

public partial class TestSetup
{
    [Fact]
    public async Task GetOutcomeById_ExistingOutcome_ShouldSucceed()
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

        // 3. Create an WorkflowOutcome
        var createOutcomeHandler = new CreateWorkflowOutcomeHandler(_context);
        var createOutcomeResult = await createOutcomeHandler.Handle(new CreateWorkflowOutcomeRequest(workflowId, null), CancellationToken.None);
        createOutcomeResult.Should().BeSuccess();
        var outcomeId = createOutcomeResult.Value;

        // 4. Prepare GetOutcomeById request
        var getOutcomeByIdHandler = new GetWorkflowOutcomeByIdHandler(_context);
        var getOutcomeByIdRequest = new GetWorkflowOutcomeByIdRequest(outcomeId);

        // Act
        var result = await getOutcomeByIdHandler.Handle(getOutcomeByIdRequest, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().NotBeNull();
        result.Value.WorkflowOutcomeId.Should().Be(outcomeId);
        result.Value.WorkflowId.Should().Be(workflowId);
        result.Value.WorkflowOutcomeState.Should().Be(EWorkflowOutcomeState.NotStarted);
        result.Value.StartedUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetOutcomeById_NonExistentOutcome_ShouldFail()
    {
        // Arrange
        var nonExistentOutcomeId = Guid.NewGuid();
        var getOutcomeByIdHandler = new GetWorkflowOutcomeByIdHandler(_context);
        var getOutcomeByIdRequest = new GetWorkflowOutcomeByIdRequest(nonExistentOutcomeId);

        // Act
        var result = await getOutcomeByIdHandler.Handle(getOutcomeByIdRequest, CancellationToken.None);

        // Assert
        result.Should().BeFailure();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be("The outcome does not exist in the database.");
    }

    [Fact]
    public async Task GetOutcomeById_MultipleOutcomes_ShouldReturnCorrectOutcome()
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
        var createOutcomeHandler = new CreateWorkflowOutcomeHandler(_context);
        var outcomeIds = new List<Guid>();
        for (int i = 0; i < 3; i++)
        {
            var createOutcomeResult = await createOutcomeHandler.Handle(new CreateWorkflowOutcomeRequest(workflowId,null), CancellationToken.None);
            createOutcomeResult.Should().BeSuccess();
            outcomeIds.Add(createOutcomeResult.Value);
        }

        // 4. Prepare GetOutcomeById request for the second outcome
        var getOutcomeByIdHandler = new GetWorkflowOutcomeByIdHandler(_context);
        var getOutcomeByIdRequest = new GetWorkflowOutcomeByIdRequest(outcomeIds[1]);

        // Act
        var result = await getOutcomeByIdHandler.Handle(getOutcomeByIdRequest, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().NotBeNull();
        result.Value.WorkflowOutcomeId.Should().Be(outcomeIds[1]);
        result.Value.WorkflowId.Should().Be(workflowId);
        result.Value.WorkflowOutcomeState.Should().Be(EWorkflowOutcomeState.NotStarted);
        result.Value.StartedUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}