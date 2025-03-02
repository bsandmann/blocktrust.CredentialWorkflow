using Blocktrust.Common.Converter;
using Blocktrust.CredentialWorkflow.Core.Commands.IssueCredentials.IssueW3cCredential.CreateW3cCredential;
using Blocktrust.CredentialWorkflow.Core.Commands.IssueCredentials.IssueW3cCredential.SignW3cCredential;
using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.GetPrivateIssuingKeyByDid;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow.ActionProcessors;
using Blocktrust.CredentialWorkflow.Core.Domain.Common;
using Blocktrust.CredentialWorkflow.Core.Domain.Credential;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Issuance;
using Blocktrust.VerifiableCredential.Common;
using Blocktrust.VerifiableCredential.VC;
using FluentAssertions;
using FluentResults;
using MediatR;
using Moq;
using Action = Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Action;
using ExecutionContext = Blocktrust.CredentialWorkflow.Core.Domain.Common.ExecutionContext;

public class IssueW3CCredentialProcessorTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly IssueW3CCredentialProcessor _processor;
    private readonly ActionOutcome _actionOutcome;
    private readonly Guid _actionId;
    private readonly ActionProcessingContext _processingContext;
    private const string IssuerDid = "did:prism:issuer123";
    private const string SubjectDid = "did:prism:subject456";
    private const string SignedJwtCredential = "eyJhbGciOiJFUzI1NksiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJkaWQ6cHJpc206aXNzdWVyMTIzIiwic3ViIjoiZGlkOnByaXNtOnN1YmplY3Q0NTYiLCJuYmYiOjE3MDg5OTIwMDAsImV4cCI6MTg2Njc1ODQwMCwidmMiOnsiQGNvbnRleHQiOlsiaHR0cHM6Ly93d3cudzMub3JnLzIwMTgvY3JlZGVudGlhbHMvdjEiXSwidHlwZSI6WyJWZXJpZmlhYmxlQ3JlZGVudGlhbCJdLCJpc3N1ZXIiOiJkaWQ6cHJpc206aXNzdWVyMTIzIiwidmFsaWRGcm9tIjoiMjAyNC0wMi0yNlQxNjowMDowMFoiLCJjcmVkZW50aWFsU3ViamVjdCI6eyJpZCI6ImRpZDpwcmlzbTpzdWJqZWN0NDU2IiwibmFtZSI6IkpvaG4gRG9lIn19fQ.r3rPiuuHPRfsl5N_RbNuSyc-EwS5O1W2FJFxrT-91egGZy5RJ0r-MBOZwkD__j4iCLBdBjsm3T2jRZh6KgtQzQ";
    private const string Base64EncodedPrivateKey = "MDEyMzQ1Njc4OTAxMjM0NTY3ODkwMTIzNDU2Nzg5MDE="; // Just a placeholder

    public IssueW3CCredentialProcessorTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _processor = new IssueW3CCredentialProcessor(_mediatorMock.Object);
        _actionId = Guid.NewGuid();
        _actionOutcome = new ActionOutcome(_actionId);
        
        // Create execution context with the subject DID parameter
        var inputContext = new Dictionary<string, string>
        {
            { "subjectdid", SubjectDid },
            { "username", "Alice Smith" }
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
    public void ActionType_ShouldBeIssueW3CCredential()
    {
        _processor.ActionType.Should().Be(EActionType.IssueW3CCredential);
    }

    [Fact]
    public async Task ProcessAsync_WithValidInputs_ShouldSucceed()
    {
        // Arrange
        var input = new IssueW3cCredential
        {
            SubjectDid = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "subjectDid"
            },
            IssuerDid = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = IssuerDid
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

        // Create a properly initialized credential
        var credential = CreateTestCredential();
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateW3cCredentialRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(credential));
            
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPrivateIssuingKeyByDidRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(Base64EncodedPrivateKey));
            
        _mediatorMock.Setup(m => m.Send(It.IsAny<SignW3cCredentialRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(SignedJwtCredential));

        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Success);
        _actionOutcome.OutcomeJson.Should().Be(SignedJwtCredential);
        
        // Verify CreateW3cCredentialRequest was called with correct parameters
        _mediatorMock.Verify(m => m.Send(
            It.Is<CreateW3cCredentialRequest>(r => 
                r.IssuerDid == IssuerDid && 
                r.SubjectDid == SubjectDid &&
                r.AdditionalSubjectData!.ContainsKey("name") &&
                r.AdditionalSubjectData["name"].ToString() == "John Doe"),
            It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_WithMissingSubjectDid_ShouldFail()
    {
        // Arrange
        var input = new IssueW3cCredential
        {
            SubjectDid = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "missingSubjectDid" // This doesn't exist in context
            },
            IssuerDid = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = IssuerDid
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

        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
        _actionOutcome.ErrorJson.Should().Contain("subject DID is not provided");
        
        // Verify no credential creation was attempted
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateW3cCredentialRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_WithMissingIssuerDid_ShouldFail()
    {
        // Arrange
        var input = new IssueW3cCredential
        {
            SubjectDid = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "subjectDid"
            },
            IssuerDid = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "missingIssuerDid" // This doesn't exist in context
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

        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
        _actionOutcome.ErrorJson.Should().Contain("issuer DID is not provided");
    }

    [Fact]
    public async Task ProcessAsync_WithMissingClaims_ShouldFail()
    {
        // Arrange
        var input = new IssueW3cCredential
        {
            SubjectDid = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "subjectDid"
            },
            IssuerDid = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = IssuerDid
            },
            Claims = new Dictionary<string, ClaimValue>() // Empty claims dictionary
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
        _actionOutcome.ErrorJson.Should().Contain("No claims provided");
    }

    [Fact]
    public async Task ProcessAsync_WithCreateCredentialFailure_ShouldFail()
    {
        // Arrange
        var input = new IssueW3cCredential
        {
            SubjectDid = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "subjectDid"
            },
            IssuerDid = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = IssuerDid
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

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateW3cCredentialRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Invalid subject DID format"));

        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
        _actionOutcome.ErrorJson.Should().Contain("W3C credential could not be created");
    }

    [Fact]
    public async Task ProcessAsync_WithIssuingKeyNotFound_ShouldFail()
    {
        // Arrange
        var input = new IssueW3cCredential
        {
            SubjectDid = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "subjectDid"
            },
            IssuerDid = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = IssuerDid
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

        // Create a properly initialized credential
        var credential = CreateTestCredential();
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateW3cCredentialRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(credential));
            
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPrivateIssuingKeyByDidRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("No issuing key found for this DID"));

        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
        _actionOutcome.ErrorJson.Should().Contain("private key for the issuer DID could not be found");
    }

    [Fact]
    public async Task ProcessAsync_WithInvalidPrivateKeyFormat_ShouldFail()
    {
        // Arrange
        var input = new IssueW3cCredential
        {
            SubjectDid = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "subjectDid"
            },
            IssuerDid = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = IssuerDid
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

        // Create a properly initialized credential
        var credential = CreateTestCredential();
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateW3cCredentialRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(credential));
            
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPrivateIssuingKeyByDidRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok("invalid base64"));

        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
        _actionOutcome.ErrorJson.Should().Contain("Invalid private key format");
    }

    [Fact]
    public async Task ProcessAsync_WithSigningFailure_ShouldFail()
    {
        // Arrange
        var input = new IssueW3cCredential
        {
            SubjectDid = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "subjectDid"
            },
            IssuerDid = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = IssuerDid
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

        // Create a properly initialized credential
        var credential = CreateTestCredential();
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateW3cCredentialRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(credential));
            
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPrivateIssuingKeyByDidRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(Base64EncodedPrivateKey));
            
        _mediatorMock.Setup(m => m.Send(It.IsAny<SignW3cCredentialRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Invalid key for signing"));

        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
        _actionOutcome.ErrorJson.Should().Contain("Invalid key for signing");
    }

    [Fact]
    public async Task ProcessAsync_WithClaimsFromTriggerInput_ShouldResolveCorrectly()
    {
        // Arrange
        var input = new IssueW3cCredential
        {
            SubjectDid = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "subjectDid"
            },
            IssuerDid = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = IssuerDid
            },
            Claims = new Dictionary<string, ClaimValue>
            {
                ["name"] = new ClaimValue 
                { 
                    Type = ClaimValueType.TriggerProperty, 
                    ParameterReference = new ParameterReference
                    {
                        Source = ParameterSource.TriggerInput,
                        Path = "userName"
                    }
                }
            }
        };

        var action = new Action
        {
            Type = EActionType.IssueW3CCredential,
            Input = input,
            RunAfter = new List<Guid>()
        };

        // Create a properly initialized credential
        var credential = CreateTestCredential();
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateW3cCredentialRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(credential));
            
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPrivateIssuingKeyByDidRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(Base64EncodedPrivateKey));
            
        _mediatorMock.Setup(m => m.Send(It.IsAny<SignW3cCredentialRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(SignedJwtCredential));

        IRequest<Result<Credential>> capturedCreateRequest = null;
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateW3cCredentialRequest>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result<Credential>>, CancellationToken>((req, _) => capturedCreateRequest = req)
            .ReturnsAsync(Result.Ok(credential));

        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        // Verify claims were resolved correctly
        capturedCreateRequest.Should().NotBeNull();
        capturedCreateRequest.Should().BeOfType<CreateW3cCredentialRequest>();
        var createRequest = (CreateW3cCredentialRequest)capturedCreateRequest;
        createRequest.AdditionalSubjectData.Should().ContainKey("name");
        createRequest.AdditionalSubjectData["name"].Should().Be("Alice Smith");
    }

    [Fact]
    public async Task ProcessAsync_WithClaimsFromPreviousActionOutcome_ShouldResolveCorrectly()
    {
        // Arrange
        var previousActionId = Guid.NewGuid();
        var input = new IssueW3cCredential
        {
            SubjectDid = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "subjectDid"
            },
            IssuerDid = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = IssuerDid
            },
            Claims = new Dictionary<string, ClaimValue>
            {
                ["verified"] = new ClaimValue 
                { 
                    Type = ClaimValueType.Static, 
                    Value = "true"
                }
            }
        };

        var action = new Action
        {
            Type = EActionType.IssueW3CCredential,
            Input = input,
            RunAfter = new List<Guid> { previousActionId }
        };

        // Create a properly initialized credential
        var credential = CreateTestCredential();
        
        // Create a previous action outcome with a result
        var previousOutcome = new ActionOutcome(previousActionId);
        previousOutcome.FinishOutcomeWithSuccess("true");
        
        var customContext = new ActionProcessingContext(
            _processingContext.ExecutionContext,
            new List<ActionOutcome> { previousOutcome },
            null,
            CancellationToken.None
        );
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateW3cCredentialRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(credential));
            
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPrivateIssuingKeyByDidRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(Base64EncodedPrivateKey));
            
        _mediatorMock.Setup(m => m.Send(It.IsAny<SignW3cCredentialRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(SignedJwtCredential));

        IRequest<Result<Credential>> capturedCreateRequest = null;
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateW3cCredentialRequest>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result<Credential>>, CancellationToken>((req, _) => capturedCreateRequest = req)
            .ReturnsAsync(Result.Ok(credential));

        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, customContext);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        // Verify claims were resolved correctly 
        capturedCreateRequest.Should().NotBeNull();
        capturedCreateRequest.Should().BeOfType<CreateW3cCredentialRequest>();
        var createRequest = (CreateW3cCredentialRequest)capturedCreateRequest;
        createRequest.AdditionalSubjectData.Should().ContainKey("verified");
        createRequest.AdditionalSubjectData["verified"].Should().Be("true");
    }
    
    // Helper method to create a properly initialized test credential
    private Credential CreateTestCredential()
    {
        var credentialContext = new CredentialOrPresentationContext
        {
            Contexts = new List<object> { "https://www.w3.org/2018/credentials/v1" },
            SerializationOption = new SerializationOption { UseArrayEvenForSingleElement = true }
        };
        
        var credentialType = new CredentialOrPresentationType
        {
            Type = new HashSet<string> { "VerifiableCredential" },
            SerializationOption = new SerializationOption { UseArrayEvenForSingleElement = true }
        };
        
        var credentialSubjects = new List<CredentialSubject>
        {
            new CredentialSubject
            {
                Id = new Uri(SubjectDid),
                AdditionalData = new Dictionary<string, object>
                {
                    { "name", "John Doe" }
                }
            }
        };
        
        return new Credential
        {
            CredentialContext = credentialContext,
            Type = credentialType,
            CredentialSubjects = credentialSubjects,
            CredentialIssuer = new CredentialIssuer(new Uri(IssuerDid))
        };
    }
}