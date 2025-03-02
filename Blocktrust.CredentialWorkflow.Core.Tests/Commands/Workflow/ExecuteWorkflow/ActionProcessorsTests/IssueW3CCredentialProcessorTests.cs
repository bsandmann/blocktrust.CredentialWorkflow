using Blocktrust.CredentialWorkflow.Core.Commands.IssueCredentials.IssueW3cCredential.CreateW3cCredential;
using Blocktrust.CredentialWorkflow.Core.Commands.IssueCredentials.IssueW3cCredential.SignW3cCredential;
using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.GetPrivateIssuingKeyByDid;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow.ActionProcessors;
using Blocktrust.CredentialWorkflow.Core.Domain.Common;
using Blocktrust.CredentialWorkflow.Core.Domain.Credential;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Issuance;
using FluentAssertions;
using FluentResults;
using MediatR;
using Moq;
using Action = Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Action;
using ExecutionContext = Blocktrust.CredentialWorkflow.Core.Domain.Common.ExecutionContext;

namespace Blocktrust.CredentialWorkflow.Core.Tests.Commands.Workflow.ExecuteWorkflow.ActionProcessorsTests;

public class IssueW3CCredentialProcessorTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly IssueW3CCredentialProcessor _processor;
    private readonly Guid _actionId;
    private readonly ActionOutcome _actionOutcome;
    private readonly ActionProcessingContext _processingContext;
    private const string TestIssuerDid = "did:prism:123456";
    private const string TestSubjectDid = "did:prism:654321";
    private const string TestPrivateKey = "c29tZVByaXZhdGVLZXlJbkJhc2U2NFVybAo";
    
    public IssueW3CCredentialProcessorTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _processor = new IssueW3CCredentialProcessor(_mediatorMock.Object);
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
    public void ActionType_ShouldBeIssueW3CCredential()
    {
        // Assert
        _processor.ActionType.Should().Be(EActionType.IssueW3CCredential);
    }
    
    [Fact]
    public async Task ProcessAsync_WithValidInputs_ShouldIssueCredential()
    {
        // Arrange
        var input = new IssueW3cCredential
        {
            IssuerDid = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = TestIssuerDid
            },
            SubjectDid = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = TestSubjectDid
            },
            Claims = new Dictionary<string, ClaimValue>
            {
                ["name"] = new ClaimValue 
                { 
                    Type = ClaimValueType.Static, 
                    Value = "John Doe" 
                }
            }
        };
        
        var action = new Action
        {
            Type = EActionType.IssueW3CCredential,
            Input = input,
            RunAfter = new List<Guid>()
        };
        
        var credential = new Credential
        {
            CredentialContext = null,
            Type = null,
            CredentialSubjects = null
        }; // Using empty constructor
        var signedJwt = "eyJhbGciOiJFUzI1NksiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJkaWQ6cHJpc206MTIzNDU2Iiwic3ViIjoiZGlkOnByaXNtOjY1NDMyMSIsInZjIjp7IkBjb250ZXh0IjpbImh0dHBzOi8vd3d3LnczLm9yZy8yMDE4L2NyZWRlbnRpYWxzL3YxIl0sInR5cGUiOlsiVmVyaWZpYWJsZUNyZWRlbnRpYWwiXSwiY3JlZGVudGlhbFN1YmplY3QiOnsibmFtZSI6IkpvaG4gRG9lIn19fQ.signature";
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateW3cCredentialRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(credential));
            
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPrivateIssuingKeyByDidRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(TestPrivateKey));
            
        _mediatorMock.Setup(m => m.Send(It.IsAny<SignW3cCredentialRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(signedJwt));
            
        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Success);
        _actionOutcome.OutcomeJson.Should().Be(signedJwt);
        
        // Verify the credential creation request
        _mediatorMock.Verify(
            m => m.Send(
                It.Is<CreateW3cCredentialRequest>(r => 
                    r.IssuerDid == TestIssuerDid && 
                    r.SubjectDid == TestSubjectDid && 
                    r.AdditionalSubjectData != null &&
                    r.AdditionalSubjectData.ContainsKey("name")),
                It.IsAny<CancellationToken>()),
            Times.Once);
        
        // Verify private key retrieval
        _mediatorMock.Verify(
            m => m.Send(
                It.Is<GetPrivateIssuingKeyByDidRequest>(r => r.Did == TestIssuerDid),
                It.IsAny<CancellationToken>()),
            Times.Once);
        
        // Verify signing request - use only the issuer DID for verification because comparing Credential instances might be problematic
        _mediatorMock.Verify(
            m => m.Send(
                It.Is<SignW3cCredentialRequest>(r => r.IssuerDid == TestIssuerDid),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
    [Fact]
    public async Task ProcessAsync_WithMissingSubjectDid_ShouldFail()
    {
        // Arrange
        var input = new IssueW3cCredential
        {
            IssuerDid = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = TestIssuerDid
            },
            SubjectDid = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "subjectDid" // Not present in execution context
            },
            Claims = new Dictionary<string, ClaimValue>
            {
                ["name"] = new ClaimValue { Type = ClaimValueType.Static, Value = "John Doe" }
            }
        };
        
        var action = new Action
        {
            Type = EActionType.IssueW3CCredential,
            Input = input,
            RunAfter = new List<Guid>()
        };
        
        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
        _actionOutcome.ErrorJson.Should().Contain("subject DID is not provided");
    }
    
    [Fact]
    public async Task ProcessAsync_WithMissingIssuerDid_ShouldFail()
    {
        // Arrange
        var input = new IssueW3cCredential
        {
            IssuerDid = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "issuerDid" // Not present in execution context
            },
            SubjectDid = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = TestSubjectDid
            },
            Claims = new Dictionary<string, ClaimValue>
            {
                ["name"] = new ClaimValue { Type = ClaimValueType.Static, Value = "John Doe" }
            }
        };
        
        var action = new Action
        {
            Type = EActionType.IssueW3CCredential,
            Input = input,
            RunAfter = new List<Guid>()
        };
        
        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
        _actionOutcome.ErrorJson.Should().Contain("issuer DID is not provided");
    }
    
    [Fact]
    public async Task ProcessAsync_WithNoClaims_ShouldFail()
    {
        // Arrange
        var input = new IssueW3cCredential
        {
            IssuerDid = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = TestIssuerDid
            },
            SubjectDid = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = TestSubjectDid
            },
            Claims = new Dictionary<string, ClaimValue>() // Empty claims
        };
        
        var action = new Action
        {
            Type = EActionType.IssueW3CCredential,
            Input = input,
            RunAfter = new List<Guid>()
        };
        
        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
        _actionOutcome.ErrorJson.Should().Contain("No claims provided for the credential");
    }
    
    [Fact]
    public async Task ProcessAsync_WithCreateCredentialFailure_ShouldFail()
    {
        // Arrange
        var input = new IssueW3cCredential
        {
            IssuerDid = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = TestIssuerDid
            },
            SubjectDid = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = TestSubjectDid
            },
            Claims = new Dictionary<string, ClaimValue>
            {
                ["name"] = new ClaimValue { Type = ClaimValueType.Static, Value = "John Doe" }
            }
        };
        
        var action = new Action
        {
            Type = EActionType.IssueW3CCredential,
            Input = input,
            RunAfter = new List<Guid>()
        };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateW3cCredentialRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Failed to create credential"));
        
        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
        _actionOutcome.ErrorJson.Should().Contain("The W3C credential could not be created");
    }
    
    [Fact]
    public async Task ProcessAsync_WithSigningFailure_ShouldFail()
    {
        // Arrange
        var input = new IssueW3cCredential
        {
            IssuerDid = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = TestIssuerDid
            },
            SubjectDid = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = TestSubjectDid
            },
            Claims = new Dictionary<string, ClaimValue>
            {
                ["name"] = new ClaimValue { Type = ClaimValueType.Static, Value = "John Doe" }
            }
        };
        
        var action = new Action
        {
            Type = EActionType.IssueW3CCredential,
            Input = input,
            RunAfter = new List<Guid>()
        };
        
        var credential = new Credential
        {
            CredentialContext = null,
            Type = null,
            CredentialSubjects = null
        }; // Using empty constructor
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateW3cCredentialRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(credential));
            
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPrivateIssuingKeyByDidRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(TestPrivateKey));
            
        _mediatorMock.Setup(m => m.Send(It.IsAny<SignW3cCredentialRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Signing failed: invalid key"));
        
        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
        _actionOutcome.ErrorJson.Should().Contain("Signing failed: invalid key");
    }
}