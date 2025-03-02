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

public class VerifyW3CCredentialProcessorTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly VerifyW3CCredentialProcessor _processor;
    private readonly ActionOutcome _actionOutcome;
    private readonly Guid _actionId;
    private readonly ActionProcessingContext _processingContext;
    
    // Sample credentials for testing
    private const string ValidJwtCredential = "eyJhbGciOiJFUzI1NksiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJkaWQ6cHJpc206aXNzdWVyMTIzIiwic3ViIjoiZGlkOnByaXNtOnN1YmplY3Q0NTYiLCJuYmYiOjE3MDg5OTIwMDAsImV4cCI6MTg2Njc1ODQwMCwidmMiOnsiQGNvbnRleHQiOlsiaHR0cHM6Ly93d3cudzMub3JnLzIwMTgvY3JlZGVudGlhbHMvdjEiXSwidHlwZSI6WyJWZXJpZmlhYmxlQ3JlZGVudGlhbCJdLCJpc3N1ZXIiOiJkaWQ6cHJpc206aXNzdWVyMTIzIiwidmFsaWRGcm9tIjoiMjAyNC0wMi0yNlQxNjowMDowMFoiLCJjcmVkZW50aWFsU3ViamVjdCI6eyJpZCI6ImRpZDpwcmlzbTpzdWJqZWN0NDU2IiwibmFtZSI6IkpvaG4gRG9lIn19fQ.r3rPiuuHPRfsl5N_RbNuSyc-EwS5O1W2FJFxrT-91egGZy5RJ0r-MBOZwkD__j4iCLBdBjsm3T2jRZh6KgtQzQ";

    public VerifyW3CCredentialProcessorTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _processor = new VerifyW3CCredentialProcessor(_mediatorMock.Object);
        _actionId = Guid.NewGuid();
        _actionOutcome = new ActionOutcome(_actionId);
        
        // Create execution context with a sample credential in the trigger input
        var inputContext = new Dictionary<string, string>
        {
            { "credential", ValidJwtCredential }
        };
        var executionContext = new ExecutionContext(Guid.NewGuid(), new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(inputContext));
        
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
                Source = ParameterSource.TriggerInput,
                Path = "credential"
            },
            CheckSignature = true,
            CheckExpiry = true,
            CheckRevocationStatus = false,
            CheckSchema = false,
            CheckTrustRegistry = false
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
        _actionOutcome.OutcomeJson.Should().Contain("IsValid=True");
        
        // Verify that the processor called the verifier with correct parameters
        _mediatorMock.Verify(m => m.Send(
            It.Is<VerifyW3CCredentialRequest>(req => 
                req.Credential == ValidJwtCredential &&
                req.VerifySignature == true &&
                req.VerifyExpiry == true &&
                req.VerifyRevocationStatus == false &&
                req.VerifySchema == false &&
                req.VerifyTrustRegistry == false),
            It.IsAny<CancellationToken>()), 
            Times.Once);
    }
    
    [Fact]
    public async Task ProcessAsync_WithCredentialFromPreviousActionOutcome_ShouldSucceed()
    {
        // Arrange
        var previousActionId = Guid.NewGuid();
        var input = new VerifyW3cCredential
        {
            CredentialReference = new ParameterReference
            {
                Source = ParameterSource.ActionOutcome,
                ActionId = previousActionId
            },
            CheckSignature = true,
            CheckExpiry = true
        };
        
        var action = new Action
        {
            Type = EActionType.VerifyW3CCredential,
            Input = input,
            RunAfter = new List<Guid> { previousActionId }
        };
        
        // Create a previous action outcome with a credential result
        var previousOutcome = new ActionOutcome(previousActionId);
        previousOutcome.FinishOutcomeWithSuccess(ValidJwtCredential);
        
        var customContext = new ActionProcessingContext(
            _processingContext.ExecutionContext,
            new List<ActionOutcome> { previousOutcome },
            null,
            CancellationToken.None
        );
        
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
        
        // Verify the processor used the credential from the previous action outcome
        _mediatorMock.Verify(m => m.Send(
            It.Is<VerifyW3CCredentialRequest>(req => 
                req.Credential == ValidJwtCredential),
            It.IsAny<CancellationToken>()), 
            Times.Once);
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
                Path = "nonexistentCredential" // This doesn't exist in context
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
        _actionOutcome.ErrorJson.Should().Contain("No credential found");
        
        // Verify no verification was attempted
        _mediatorMock.Verify(m => m.Send(It.IsAny<VerifyW3CCredentialRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task ProcessAsync_WithInvalidCredential_ShouldFail()
    {
        // Arrange
        var input = new VerifyW3cCredential
        {
            CredentialReference = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "credential"
            }
        };
        
        var action = new Action
        {
            Type = EActionType.VerifyW3CCredential,
            Input = input,
            RunAfter = new List<Guid>()
        };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<VerifyW3CCredentialRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Invalid credential format"));
        
        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
        _actionOutcome.ErrorJson.Should().Contain("Invalid credential format");
    }
    
    [Fact]
    public async Task ProcessAsync_WithExpiredCredential_ShouldReportValidationResults()
    {
        // Arrange
        var input = new VerifyW3cCredential
        {
            CredentialReference = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "credential"
            },
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
            IsExpired = true, // Credential is expired
            IsRevoked = false,
            InTrustRegistry = true
        };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<VerifyW3CCredentialRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(verificationResult));
        
        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);
        
        // Assert
        result.IsSuccess.Should().BeTrue(); // Process succeeded even though credential is invalid
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Success);
        _actionOutcome.OutcomeJson.Should().Contain("IsValid=False"); // But the verification result shows it's invalid
    }
    
    [Fact]
    public async Task ProcessAsync_WithAllVerificationOptions_ShouldPassCorrectParameters()
    {
        // Arrange
        var input = new VerifyW3cCredential
        {
            CredentialReference = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "credential"
            },
            CheckSignature = true,
            CheckExpiry = true,
            CheckRevocationStatus = true,
            CheckSchema = true,
            CheckTrustRegistry = true
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
        
        VerifyW3CCredentialRequest capturedRequest = null;
        _mediatorMock.Setup(m => m.Send(It.IsAny<VerifyW3CCredentialRequest>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result<CredentialVerificationResult>>, CancellationToken>((req, _) => capturedRequest = (VerifyW3CCredentialRequest)req)
            .ReturnsAsync(Result.Ok(verificationResult));
        
        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        
        // Verify all verification options were passed correctly
        capturedRequest.Should().NotBeNull();
        capturedRequest.VerifySignature.Should().BeTrue();
        capturedRequest.VerifyExpiry.Should().BeTrue();
        capturedRequest.VerifyRevocationStatus.Should().BeTrue();
        capturedRequest.VerifySchema.Should().BeTrue();
        capturedRequest.VerifyTrustRegistry.Should().BeTrue();
    }
}