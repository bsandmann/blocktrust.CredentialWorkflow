using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.CreateTenant;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.CreateWorkflow;
using Blocktrust.CredentialWorkflow.Core.Domain.Enums;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Blocktrust.CredentialWorkflow.Core.Tests;

using Core.Commands.WorkflowOutcome.CreateWorkflowOutcome;
using Core.Commands.WorkflowOutcome.UpdateWorkflowOutcome;

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

        // 3. Create an WorkflowOutcome
        var createOutcomeHandler = new CreateWorkflowOutcomeHandler(_context);
        var createOutcomeResult = await createOutcomeHandler.Handle(new CreateWorkflowOutcomeRequest(workflowId, null), CancellationToken.None);
        createOutcomeResult.Should().BeSuccess();
        var outcomeId = createOutcomeResult.Value;

        // 4. Prepare UpdateOutcome request
        var updateOutcomeHandler = new UpdateWorkflowOutcomeHandler(_context);
        var updateOutcomeRequest = new UpdateWorkflowOutcomeRequest(
            outcomeId,
            EWorkflowOutcomeState.Success,
            "{\"result\": \"success\"}",
            null
        );

        // Act
        var result = await updateOutcomeHandler.Handle(updateOutcomeRequest, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().NotBeNull();
        result.Value.WorkflowOutcomeId.Should().Be(outcomeId);
        result.Value.WorkflowId.Should().Be(workflowId);
        result.Value.WorkflowOutcomeState.Should().Be(EWorkflowOutcomeState.Success);
        result.Value.ActionOutcomesJson.Should().Be("{\"result\": \"success\"}");
        result.Value.ErrorJson.Should().BeNull();
        result.Value.EndedUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task UpdateOutcome_NonExistentOutcome_ShouldFail()
    {
        // Arrange
        var nonExistentOutcomeId = Guid.NewGuid();
        var updateOutcomeHandler = new UpdateWorkflowOutcomeHandler(_context);
        var updateOutcomeRequest = new UpdateWorkflowOutcomeRequest(
            nonExistentOutcomeId,
            EWorkflowOutcomeState.Success,
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

        // 3. Create an WorkflowOutcome
        var createOutcomeHandler = new CreateWorkflowOutcomeHandler(_context);
        var createOutcomeResult = await createOutcomeHandler.Handle(new CreateWorkflowOutcomeRequest(workflowId, null), CancellationToken.None);
        createOutcomeResult.Should().BeSuccess();
        var outcomeId = createOutcomeResult.Value;

        // 4. Prepare UpdateOutcome request
        var updateOutcomeHandler = new UpdateWorkflowOutcomeHandler(_context);
        var updateOutcomeRequest = new UpdateWorkflowOutcomeRequest(
            outcomeId,
            EWorkflowOutcomeState.FailedWithErrors,
            null,
            "{\"error\": \"Something went wrong\"}"
        );

        // Act
        var result = await updateOutcomeHandler.Handle(updateOutcomeRequest, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().NotBeNull();
        result.Value.WorkflowOutcomeId.Should().Be(outcomeId);
        result.Value.WorkflowId.Should().Be(workflowId);
        result.Value.WorkflowOutcomeState.Should().Be(EWorkflowOutcomeState.FailedWithErrors);
        result.Value.ActionOutcomesJson.Should().BeNull();
        result.Value.ErrorJson.Should().Be("{\"error\": \"Something went wrong\"}");
        result.Value.EndedUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}