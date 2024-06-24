using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.CreateTenant;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.CreateWorkflow;
using Blocktrust.CredentialWorkflow.Core.Commands.Outcome.CreateOutcome;
using Blocktrust.CredentialWorkflow.Core.Commands.Outcome.UpdateOutcome;
using Blocktrust.CredentialWorkflow.Core.Domain.Enums;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Blocktrust.CredentialWorkflow.Core.Tests;

public partial class TestSetup
{
    [Fact]
    public async Task UpdateOutcome_ExistingOutcome_ShouldSucceed()
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

        // 3. Create an Outcome
        var createOutcomeHandler = new CreateOutcomeHandler(_context);
        var createOutcomeResult = await createOutcomeHandler.Handle(new CreateOutcomeRequest(workflowId), CancellationToken.None);
        createOutcomeResult.Should().BeSuccess();
        var outcomeId = createOutcomeResult.Value;

        // 4. Prepare UpdateOutcome request
        var updateOutcomeHandler = new UpdateOutcomeHandler(_context);
        var updateOutcomeRequest = new UpdateOutcomeRequest(
            outcomeId,
            EOutcomeState.Success,
            "{\"result\": \"success\"}",
            null
        );

        // Act
        var result = await updateOutcomeHandler.Handle(updateOutcomeRequest, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().NotBeNull();
        result.Value.OutcomeId.Should().Be(outcomeId);
        result.Value.WorkflowId.Should().Be(workflowId);
        result.Value.OutcomeState.Should().Be(EOutcomeState.Success);
        result.Value.OutcomeJson.Should().Be("{\"result\": \"success\"}");
        result.Value.ErrorJson.Should().BeNull();
        result.Value.EndedUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task UpdateOutcome_NonExistentOutcome_ShouldFail()
    {
        // Arrange
        var nonExistentOutcomeId = Guid.NewGuid();
        var updateOutcomeHandler = new UpdateOutcomeHandler(_context);
        var updateOutcomeRequest = new UpdateOutcomeRequest(
            nonExistentOutcomeId,
            EOutcomeState.Success,
            "{\"result\": \"success\"}",
            null
        );

        // Act
        var result = await updateOutcomeHandler.Handle(updateOutcomeRequest, CancellationToken.None);

        // Assert
        result.Should().BeFailure();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be("The outcome does not exist in the database. The outcome cannot be updated.");
    }

    [Fact]
    public async Task UpdateOutcome_ToFailedState_ShouldUpdateEndedUtcAndErrorJson()
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

        // 3. Create an Outcome
        var createOutcomeHandler = new CreateOutcomeHandler(_context);
        var createOutcomeResult = await createOutcomeHandler.Handle(new CreateOutcomeRequest(workflowId), CancellationToken.None);
        createOutcomeResult.Should().BeSuccess();
        var outcomeId = createOutcomeResult.Value;

        // 4. Prepare UpdateOutcome request
        var updateOutcomeHandler = new UpdateOutcomeHandler(_context);
        var updateOutcomeRequest = new UpdateOutcomeRequest(
            outcomeId,
            EOutcomeState.FailedWithErrors,
            null,
            "{\"error\": \"Something went wrong\"}"
        );

        // Act
        var result = await updateOutcomeHandler.Handle(updateOutcomeRequest, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().NotBeNull();
        result.Value.OutcomeId.Should().Be(outcomeId);
        result.Value.WorkflowId.Should().Be(workflowId);
        result.Value.OutcomeState.Should().Be(EOutcomeState.FailedWithErrors);
        result.Value.OutcomeJson.Should().BeNull();
        result.Value.ErrorJson.Should().Be("{\"error\": \"Something went wrong\"}");
        result.Value.EndedUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task UpdateOutcome_ToInProgressState_ShouldNotUpdateEndedUtc()
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

        // 3. Create an Outcome
        var createOutcomeHandler = new CreateOutcomeHandler(_context);
        var createOutcomeResult = await createOutcomeHandler.Handle(new CreateOutcomeRequest(workflowId), CancellationToken.None);
        createOutcomeResult.Should().BeSuccess();
        var outcomeId = createOutcomeResult.Value;

        // 4. Prepare UpdateOutcome request
        var updateOutcomeHandler = new UpdateOutcomeHandler(_context);
        var updateOutcomeRequest = new UpdateOutcomeRequest(
            outcomeId,
            EOutcomeState.Running,
            "{\"progress\": \"50%\"}",
            null
        );

        // Act
        var result = await updateOutcomeHandler.Handle(updateOutcomeRequest, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().NotBeNull();
        result.Value.OutcomeId.Should().Be(outcomeId);
        result.Value.WorkflowId.Should().Be(workflowId);
        result.Value.OutcomeState.Should().Be(EOutcomeState.Running);
        result.Value.OutcomeJson.Should().Be("{\"progress\": \"50%\"}");
        result.Value.ErrorJson.Should().BeNull();
        result.Value.EndedUtc.Should().BeNull();
    }
}