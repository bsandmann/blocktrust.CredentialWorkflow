using System.Threading;
using System.Net.Http;
using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.GetTenantInformation;
using Xunit;

namespace Blocktrust.CredentialWorkflow.Core.Tests.Commands.Workflow.ExecuteWorkflow.ActionProcessorsTests;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow.ActionProcessors;
using Blocktrust.CredentialWorkflow.Core.Domain.Common;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.DID;
using Blocktrust.CredentialWorkflow.Core.Domain.Tenant;
using Blocktrust.CredentialWorkflow.Core.Services;
using FluentAssertions;
using FluentResults;
using MediatR;
using Moq;
using Action = Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Action;
using ExecutionContext = Blocktrust.CredentialWorkflow.Core.Domain.Common.ExecutionContext;
using r = FluentResults.Result;

public class UpdateDIDActionProcessorTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly UpdateDIDActionProcessor _processor;
    private readonly Guid _actionId;
    private readonly ActionOutcome _actionOutcome;
    private readonly ActionProcessingContext _processingContext;
    private readonly Guid _tenantId = Guid.NewGuid();
    
    public UpdateDIDActionProcessorTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _processor = new UpdateDIDActionProcessor(_mediatorMock.Object, _httpClientFactoryMock.Object);
        _actionId = Guid.NewGuid();
        _actionOutcome = new ActionOutcome(_actionId);
        
        // Create workflow with tenant ID and required properties
        var workflow = new Domain.Workflow.Workflow
        {
            TenantId = _tenantId,
            Name = "Test Workflow",
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
            WorkflowState = Domain.Enums.EWorkflowState.ActiveWithExternalTrigger,
            WorkflowOutcomes = new List<Domain.Workflow.WorkflowOutcome>()
        };
        
        // Create execution context with a valid DID
        var inputContext = new Dictionary<string, string>
        {
            { "did", "did:prism:123456789abcdef" },
            { "opType", "Add" },
            { "keyId", "key-1" },
            { "purpose", "authentication" },
            { "curve", "secp256k1" }
        };
        var executionContext = new ExecutionContext(Guid.NewGuid(), new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(inputContext));
        
        _processingContext = new ActionProcessingContext(
            executionContext,
            new List<ActionOutcome>(),
            workflow,
            CancellationToken.None
        );
        
        // Setup mock tenant response
        var tenant = new Tenant
        {
            TenantId = _tenantId,
            Name = "Test Tenant",
            CreatedUtc = DateTime.UtcNow,
            OpnRegistrarUrl = "https://registrar.example.com",
            WalletId = "wallet123"
        };
        
        var tenantResponse = new GetTenantInformationResponse { Tenant = tenant };
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetTenantInformationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(tenantResponse));
    }
    
    [Fact]
    public void ActionType_ShouldBeUpdateDID()
    {
        // Assert
        _processor.ActionType.Should().Be(EActionType.UpdateDID);
    }
    
    [Fact]
    public async Task ProcessAsync_WithMissingDid_ShouldFail()
    {
        // Arrange
        var input = new UpdateDIDAction
        {
            UseTenantRegistrar = true,
            Did = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "nonexistentDid" // Doesn't exist in context
            },
            UpdateOperations = new List<DIDUpdateOperation>
            {
                new()
                {
                    OperationType = new ParameterReference
                    {
                        Source = ParameterSource.TriggerInput,
                        Path = "opType"
                    }
                }
            }
        };
        
        var action = new Action
        {
            Type = EActionType.UpdateDID,
            Input = input,
            RunAfter = new List<Guid>()
        };
        
        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
        _actionOutcome.ErrorJson.Should().Contain("No DID provided for update operation");
    }
    
    [Fact]
    public async Task ProcessAsync_WithMissingUpdateOperations_ShouldFail()
    {
        // Arrange
        var input = new UpdateDIDAction
        {
            UseTenantRegistrar = true,
            Did = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "did"
            },
            UpdateOperations = new List<DIDUpdateOperation>() // Empty list
        };
        
        var action = new Action
        {
            Type = EActionType.UpdateDID,
            Input = input,
            RunAfter = new List<Guid>()
        };
        
        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
        _actionOutcome.ErrorJson.Should().Contain("At least one operation is required");
    }

}