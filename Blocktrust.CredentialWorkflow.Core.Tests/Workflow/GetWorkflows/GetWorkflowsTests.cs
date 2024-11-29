// namespace Blocktrust.CredentialWorkflow.Core.Tests;
//
// using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.CreateTenant;
// using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.CreateWorkflow;
// using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.GetWorkflows;
// using Blocktrust.CredentialWorkflow.Core.Entities.Workflow;
// using Blocktrust.CredentialWorkflow.Core.Entities.Outcomes;
// using Blocktrust.CredentialWorkflow.Core.Domain.Enums;
// using Commands.Outcomes.CreateOutcome;
// using FluentAssertions;
// using FluentResults.Extensions.FluentAssertions;
// using Microsoft.EntityFrameworkCore;
//
// public partial class TestSetup
// {
//     [Fact]
//     public async Task GetWorkflows_MultipleWorkflowsWithOutcomes_ShouldReturnAllWorkflowsWithLastOutcome()
//     {
//         // Arrange
//         // 1. Create a Tenant
//         var createTenantHandler = new CreateTenantHandler(_context);
//         var createTenantResult = await createTenantHandler.Handle(new CreateTenantRequest("TestTenant"), CancellationToken.None);
//         createTenantResult.Should().BeSuccess();
//         var tenantId = createTenantResult.Value;
//
//         // 2. Create multiple Workflows
//         var createWorkflowHandler = new CreateWorkflowHandler(_context);
//         var workflow1Result = await createWorkflowHandler.Handle(new CreateWorkflowRequest(tenantId), CancellationToken.None);
//         var workflow2Result = await createWorkflowHandler.Handle(new CreateWorkflowRequest(tenantId), CancellationToken.None);
//         workflow1Result.Should().BeSuccess();
//         workflow2Result.Should().BeSuccess();
//
//         // 3. Add Outcomes to Workflows
//         var workflow1 = await _context.WorkflowEntities.FindAsync(workflow1Result.Value.WorkflowId);
//         var workflow2 = await _context.WorkflowEntities.FindAsync(workflow2Result.Value.WorkflowId);
//
//         var createOutcomeHandler = new CreateOutcomeHandler(_context);
//         createOutcomeHandler.Handle(new CreateOutcomeRequest(), CancellationToken.None);
//         
//         workflow1!.OutcomeEntities.Add(new OutcomeEntity { EndedUtc = DateTime.UtcNow.AddHours(-2),OutcomeState = EOutcomeState.Success });
//         workflow1.OutcomeEntities.Add(new OutcomeEntity { EndedUtc = DateTime.UtcNow.AddHours(-1), OutcomeState = EOutcomeState.FailedWithErrors });
//         workflow2!.OutcomeEntities.Add(new OutcomeEntity { EndedUtc = DateTime.UtcNow, OutcomeState = EOutcomeState.Success });
//
//         await _context.SaveChangesAsync();
//
//         // 4. Prepare GetWorkflows request
//         var getWorkflowsHandler = new GetWorkflowsHandler(_context);
//         var getWorkflowsRequest = new GetWorkflowsRequest(createTenantResult.Value);
//
//         // Act
//         var result = await getWorkflowsHandler.Handle(getWorkflowsRequest, CancellationToken.None);
//
//         // Assert
//         result.Should().BeSuccess();
//         result.Value.Should().NotBeNull();
//         result.Value.Should().HaveCount(2);
//
//         var workflow1ResultxWithLastResult = result.Value.First(w => w.WorkflowId == workflow1Result.Value.WorkflowId);
//         workflow1ResultxWithLastResult.Should().NotBeNull();
//         workflow1ResultxWithLastResult.Name.Should().Be("New Workflow");
//         workflow1ResultxWithLastResult.WorkflowState.Should().Be(EWorkflowState.Inactive);
//         workflow1ResultxWithLastResult.LastOutcome.Should().NotBeNull();
//         workflow1ResultxWithLastResult.LastOutcome!.OutcomeState.Should().Be(EOutcomeState.FailedWithErrors);
//
//         var workflow2ResultWithLastResult = result.Value.First(w => w.WorkflowId == workflow2Result.Value.WorkflowId);
//         workflow2ResultWithLastResult.Should().NotBeNull();
//         workflow2ResultWithLastResult.Name.Should().Be("New Workflow");
//         workflow2ResultWithLastResult.WorkflowState.Should().Be(EWorkflowState.Inactive);
//         workflow2ResultWithLastResult.LastOutcome.Should().NotBeNull();
//         workflow2ResultWithLastResult.LastOutcome!.OutcomeState.Should().Be(EOutcomeState.Success);
//     }
// }