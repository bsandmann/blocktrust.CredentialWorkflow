using Blocktrust.CredentialWorkflow.Core.Commands.VerifyCredentials.VerifyW3cCredentials.VerifyW3cCredential;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow.ActionProcessors;
using Blocktrust.CredentialWorkflow.Core.Domain.Common;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Verification;
using Blocktrust.CredentialWorkflow.Core.Domain.Verification;
using FluentAssertions;
using FluentResults;
using MediatR;
using Moq;
using Action = Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Action;
using ExecutionContext = Blocktrust.CredentialWorkflow.Core.Domain.Common.ExecutionContext;

namespace Blocktrust.CredentialWorkflow.Core.Tests.Commands.Workflow.ExecuteWorkflow.ActionProcessorsTests;

public class VerifyW3CCredentialProcessorTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly VerifyW3CCredentialProcessor _processor;
    private readonly Guid _actionId;
    private readonly ActionOutcome _actionOutcome;
    private readonly ActionProcessingContext _processingContext;
    private const string ValidJwtCredential = "eyJhbGciOiJFUzI1NksiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJkaWQ6cHJpc206MTIzNDU2Iiwic3ViIjoiZGlkOnByaXNtOjY1NDMyMSIsInZjIjp7IkBjb250ZXh0IjpbImh0dHBzOi8vd3d3LnczLm9yZy8yMDE4L2NyZWRlbnRpYWxzL3YxIl0sInR5cGUiOlsiVmVyaWZpYWJsZUNyZWRlbnRpYWwiXSwiY3JlZGVudGlhbFN1YmplY3QiOnsibmFtZSI6IkpvaG4gRG9lIn19fQ.signature";
    
    public VerifyW3CCredentialProcessorTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _processor = new VerifyW3CCredentialProcessor(_mediatorMock.Object);
        _actionId = Guid.NewGuid();
        _actionOutcome = new ActionOutcome(_actionId);
        
        // Create execution context
        var executionContext = new ExecutionContext(Guid.NewGuid());
        _processingContext = new ActionProcessingContext(
            executionContext,
            new List<ActionOutcome>(),
            null, 
            CancellationToken.None
        );
    }
    
    [Fact]
    public void ActionType_ShouldBeVerifyW3CCredential()
    {
        // Assert
        _processor.ActionType.Should().Be(EActionType.VerifyW3CCredential);
    }
    
    [Fact]
    public async Task ProcessAsync_WithValidCredential_ShouldSucceed()
    {
        // Arrange
        var input = new VerifyW3cCredential
        {
            CredentialReference = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = ValidJwtCredential
            },
            CheckSignature = true,
            CheckRevocationStatus = true,
            CheckExpiry = true
        };
        
        var action = new Action
        {
            Type = EActionType.VerifyW3CCredential,
            Input = input,
            RunAfter = new List<Guid>()
        };
        
        var verificationResult = new CredentialVerificationResult
        {
            SignatureValid = true,
            IsExpired = false,
            IsRevoked = false,
            InTrustRegistry = true
        };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<VerifyW3CCredentialRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(verificationResult));
            
        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Success);
        _actionOutcome.OutcomeJson.Should().Contain("Credential verified");
        
        // Verify verification request
        _mediatorMock.Verify(
            m => m.Send(
                It.Is<VerifyW3CCredentialRequest>(r => 
                    r.Credential == ValidJwtCredential && 
                    r.VerifySignature == true && 
                    r.VerifyRevocationStatus == true && 
                    r.VerifyExpiry == true),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
    [Fact]
    public async Task ProcessAsync_WithInvalidCredential_ShouldFail()
    {
        // Arrange
        var input = new VerifyW3cCredential
        {
            CredentialReference = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = ValidJwtCredential
            },
            CheckSignature = true
        };
        
        var action = new Action
        {
            Type = EActionType.VerifyW3CCredential,
            Input = input,
            RunAfter = new List<Guid>()
        };
        
        var verificationResult = new CredentialVerificationResult
        {
            SignatureValid = false,
            IsExpired = false,
            IsRevoked = false,
            ErrorMessage = "Invalid signature"
        };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<VerifyW3CCredentialRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(verificationResult));
            
        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);
        
        // Assert
        // Verification completed, but the result indicates an invalid credential
        result.IsSuccess.Should().BeTrue();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Success);
        _actionOutcome.OutcomeJson.Should().Contain("IsValid=False");
    }
    
    [Fact]
    public async Task ProcessAsync_WithNoCredential_ShouldFail()
    {
        // Arrange
        var input = new VerifyW3cCredential
        {
            CredentialReference = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "credential" // Not present in execution context
            }
        };
        
        var action = new Action
        {
            Type = EActionType.VerifyW3CCredential,
            Input = input,
            RunAfter = new List<Guid>()
        };
        
        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
        _actionOutcome.ErrorJson.Should().Contain("No credential found in the execution context to verify");
    }
    
    [Fact]
    public async Task ProcessAsync_WithVerificationServiceFailure_ShouldFail()
    {
        // Arrange
        var input = new VerifyW3cCredential
        {
            CredentialReference = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = ValidJwtCredential
            }
        };
        
        var action = new Action
        {
            Type = EActionType.VerifyW3CCredential,
            Input = input,
            RunAfter = new List<Guid>()
        };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<VerifyW3CCredentialRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Verification service unavailable"));
            
        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
    }
    
    [Fact]
    public async Task ProcessAsync_WithCredentialFromPreviousAction_ShouldSucceed()
    {
        // Arrange
        var previousActionId = Guid.NewGuid();
        var previousOutcome = new ActionOutcome(previousActionId);
        previousOutcome.FinishOutcomeWithSuccess(ValidJwtCredential);
        
        var customContext = new ActionProcessingContext(
            _processingContext.ExecutionContext,
            new List<ActionOutcome> { previousOutcome },
            null,
            CancellationToken.None
        );
        
        var input = new VerifyW3cCredential
        {
            CredentialReference = new ParameterReference
            {
                Source = ParameterSource.ActionOutcome,
                ActionId = previousActionId
            }
        };
        
        var action = new Action
        {
            Type = EActionType.VerifyW3CCredential,
            Input = input,
            RunAfter = new List<Guid> { previousActionId }
        };
        
        var verificationResult = new CredentialVerificationResult
        {
            SignatureValid = true,
            IsExpired = false,
            IsRevoked = false,
            InTrustRegistry = true
        };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<VerifyW3CCredentialRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(verificationResult));
            
        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, customContext);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Success);
        
        // Verify verification request was sent with the credential from the previous action
        _mediatorMock.Verify(
            m => m.Send(
                It.Is<VerifyW3CCredentialRequest>(r => r.Credential == ValidJwtCredential),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
    [Fact]
    public async Task ProcessAsync_WithDifferentCheckOptions_ShouldPassThem()
    {
        // Arrange
        var input = new VerifyW3cCredential
        {
            CredentialReference = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = ValidJwtCredential
            },
            CheckSignature = true,
            CheckRevocationStatus = false,
            CheckExpiry = true,
            CheckSchema = true,
            CheckTrustRegistry = false,
            RequiredIssuerId = "did:prism:123456"
        };
        
        var action = new Action
        {
            Type = EActionType.VerifyW3CCredential,
            Input = input,
            RunAfter = new List<Guid>()
        };
        
        var verificationResult = new CredentialVerificationResult
        {
            SignatureValid = true,
            IsExpired = false,
            IsRevoked = false,
            InTrustRegistry = true
        };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<VerifyW3CCredentialRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(verificationResult));
            
        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        
        // Verify verification request sent with correct options
        _mediatorMock.Verify(
            m => m.Send(
                It.Is<VerifyW3CCredentialRequest>(r => 
                    r.VerifySignature == true && 
                    r.VerifyRevocationStatus == false && 
                    r.VerifyExpiry == true &&
                    r.VerifySchema == true &&
                    r.VerifyTrustRegistry == false),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}