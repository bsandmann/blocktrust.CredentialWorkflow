using Blocktrust.CredentialWorkflow.Core.Commands.ValidateCredentials.W3cValidation;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow.ActionProcessors;
using Blocktrust.CredentialWorkflow.Core.Domain.Common;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Validation;
using FluentAssertions;
using FluentResults;
using MediatR;
using Moq;

namespace Blocktrust.CredentialWorkflow.Core.Tests.Commands.Workflow.ExecuteWorkflow.ActionProcessorsTests;

public class W3cValidationProcessorTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly W3cValidationProcessor _processor;
    private readonly ActionOutcome _actionOutcome;
    private readonly Guid _actionId;
    private readonly Guid _tenantId;
    private readonly Mock<ActionProcessingContext> _processingContextMock;

    private const string ValidJwtCredential = "eyJhbGciOiJFUzI1NksiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJkaWQ6cHJpc206YzBkNDc4NWM1NjQxODM5MzNhM2FiNTk2YzE1NWQwNWMyYmNmZGI4ZGYxNDA1NGQ1NTczZTFhZTE5YWIxMzE0NTpDcXdEQ3FrREVsb0tCV3RsZVMweEVBUkNUd29KYzJWamNESTFObXN4RWlDMEx3eW5kOWNoeVR4UTg2enhaOUd3RHduNzZ3Yk8wM3lDV0h1SzhwUDZrQm9ndWI5c19rNnY0cGxpMVFqa2ZKb05WdG5CMExmVXNSMGJIb1F1ZzItbUQwNFNXZ29GYTJWNUxUSVFBMEpQQ2dselpXTndNalUyYXpFU0lNd3d2QTc5MjJSdVBTXzlFUTBMOXViWDhSbXFiZG12NTFTNUYweDVaNmtDR2lBS2VUdkZfRWUwV1c5cjhMN2QwRFhjSXVMYk9mdHZ5cGZzdTB3bkRDZm0weEphQ2dWclpYa3RNeEFDUWs4S0NYTmxZM0F5TlRack1SSWd2SHhXbEZHUTVLTkg2ZTVmaHJ2dS1KRjJNWkdPSHlfSlVpdVBWV0V6b0dVYUlMcE9wVWVla28yT3pabWF0UTJZZDFGeGFNUVh6eS00WnB2b0FGQ3BORUNJRWx3S0IyMWhjM1JsY2pBUUFVSlBDZ2x6WldOd01qVTJhekVTSU16blFUd0JkcTUwNmk1ZEJvZ3dNbW5Kc2ZHUEpzcDdlRVY5TldwVHo4UEdHaUMtUnVRNVlPdVQ1MmJrVWVKWXpUcEFJdEljUlJSczFXT3kwTDkyTWpUYjlobzFDZ2x6WlhKMmFXTmxMVEVTRFV4cGJtdGxaRVJ2YldGcGJuTWFHVnNpYUhSMGNITTZMeTl6YjIxbExuTmxjblpwWTJVdklsMCIsInN1YiI6ImRpZDpwcmlzbToxMjMxMjMiLCJuYmYiOjE3NDA0MTk2NTAsImV4cCI6MTg5ODE4NjA1MCwidmMiOnsiQGNvbnRleHQiOlsiaHR0cHM6Ly93d3cudzMub3JnLzIwMTgvY3JlZGVudGlhbHMvdjEiXSwidHlwZSI6WyJWZXJpZmlhYmxlQ3JlZGVudGlhbCJdLCJpc3N1ZXIiOiJkaWQ6cHJpc206YzBkNDc4NWM1NjQxODM5MzNhM2FiNTk2YzE1NWQwNWMyYmNmZGI4ZGYxNDA1NGQ1NTczZTFhZTE5YWIxMzE0NTpDcXdEQ3FrREVsb0tCV3RsZVMweEVBUkNUd29KYzJWamNESTFObXN4RWlDMEx3eW5kOWNoeVR4UTg2enhaOUd3RHduNzZ3Yk8wM3lDV0h1SzhwUDZrQm9ndWI5c19rNnY0cGxpMVFqa2ZKb05WdG5CMExmVXNSMGJIb1F1ZzItbUQwNFNXZ29GYTJWNUxUSVFBMEpQQ2dselpXTndNalUyYXpFU0lNd3d2QTc5MjJSdVBTXzlFUTBMOXViWDhSbXFiZG12NTFTNUYweDVaNmtDR2lBS2VUdkZfRWUwV1c5cjhMN2QwRFhjSXVMYk9mdHZ5cGZzdTB3bkRDZm0weEphQ2dWclpYa3RNeEFDUWs4S0NYTmxZM0F5TlRack1SSWd2SHhXbEZHUTVLTkg2ZTVmaHJ2dS1KRjJNWkdPSHlfSlVpdVBWV0V6b0dVYUlMcE9wVWVla28yT3pabWF0UTJZZDFGeGFNUVh6eS00WnB2b0FGQ3BORUNJRWx3S0IyMWhjM1JsY2pBUUFVSlBDZ2x6WldOd01qVTJhekVTSU16blFUd0JkcTUwNmk1ZEJvZ3dNbW5Kc2ZHUEpzcDdlRVY5TldwVHo4UEdHaUMtUnVRNVlPdVQ1MmJrVWVKWXpUcEFJdEljUlJSczFXT3kwTDkyTWpUYjlobzFDZ2x6WlhKMmFXTmxMVEVTRFV4cGJtdGxaRVJ2YldGcGJuTWFHVnNpYUhSMGNITTZMeTl6YjIxbExuTmxjblpwWTJVdklsMCIsInZhbGlkRnJvbSI6IjIwMjUtMDItMjRUMTc6NTQ6MDkuMzU2NTg0MiIsImNyZWRlbnRpYWxTdWJqZWN0Ijp7ImlkIjoiZGlkOnByaXNtOjEyMzEyMyIsImFnZSI6IjMzIiwidGhlbmFtZSI6Im15IG5hbWUifX19.7ftrLGy-h9BCQ6Utph1x9LXRPgM0K9vZ0v1qK4vTNA3fB5M3AXFrNBzZFOZHd-rqKG2uA9OZ-mVrvM1rM_pVzQ";
    private const string ValidJsonCredential = "{\n  \"@context\": [\n    \"https://www.w3.org/2018/credentials/v1\"\n  ],\n  \"type\": [\n    \"VerifiableCredential\"\n  ],\n  \"issuer\": \"did:prism:c0d4785c564183933a3ab596c155d05c2bcfdb8df14054d5573e1ae19ab13145\",\n  \"issuanceDate\": \"2025-02-24T17:54:09.356584Z\",\n  \"credentialSubject\": {\n    \"id\": \"did:prism:123123\",\n    \"age\": \"33\",\n    \"thename\": \"my name\"\n  }\n}";

    public W3cValidationProcessorTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _processor = new W3cValidationProcessor(_mediatorMock.Object);
        _actionId = Guid.NewGuid();
        _actionOutcome = new ActionOutcome(_actionId);
        _tenantId = Guid.NewGuid();
            
        // Mock the processing context instead of creating it directly
        _processingContextMock = new Mock<ActionProcessingContext>(
            Mock.Of<object>(),
            new List<ActionOutcome>(),
            Mock.Of<Domain.Workflow.Workflow>(),
            CancellationToken.None);
    }

    [Fact]
    public void ActionType_ShouldBeW3cValidation()
    {
        // Act & Assert
        _processor.ActionType.Should().Be(EActionType.W3cValidation);
    }

    [Fact]
    public async Task ProcessAsync_WithValidCredential_ShouldSucceed()
    {
        // Arrange
        var validationRules = new List<ValidationRule>
        {
            new ValidationRule { Type = "Required", Configuration = "credentialSubject.id" }
        };

        var input = new W3cValidationAction
        {
            CredentialReference = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = ValidJwtCredential
            },
            ValidationRules = validationRules
        };

        var action = new Domain.ProcessFlow.Actions.Action
        {
            Type = EActionType.W3cValidation,
            Input = input,
            RunAfter = new List<Guid>()
        };

        var validationResponse = new W3cValidationResponse { IsValid = true };
        _mediatorMock.Setup(m => m.Send(It.IsAny<W3cValidationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(validationResponse));

        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContextMock.Object);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _actionOutcome.ActionId.Should().Be(_actionId);
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Success);
        _actionOutcome.ErrorJson.Should().BeNull();
    }

    [Fact]
    public async Task ProcessAsync_WithInvalidCredential_ShouldFail()
    {
        // Arrange
        var validationRules = new List<ValidationRule>
        {
            new ValidationRule { Type = "Required", Configuration = "credentialSubject.id" }
        };

        var input = new W3cValidationAction
        {
            CredentialReference = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = ValidJwtCredential
            },
            ValidationRules = validationRules
        };

        var action = new Domain.ProcessFlow.Actions.Action
        {
            Type = EActionType.W3cValidation,
            Input = input,
            RunAfter = new List<Guid>()
        };

        var errors = new List<W3cValidationError>
        {
            new W3cValidationError("Required", "Field 'credentialSubject.id' is missing")
        };
            
        var validationResponse = new W3cValidationResponse 
        { 
            IsValid = false, 
            Errors = errors 
        };
            
        _mediatorMock.Setup(m => m.Send(It.IsAny<W3cValidationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(validationResponse));

        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContextMock.Object);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _actionOutcome.ActionId.Should().Be(_actionId);
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
        _actionOutcome.ErrorJson.Should().NotBeNull();
        _actionOutcome.ErrorJson.Should().Contain("Required: Field 'credentialSubject.id' is missing");
    }

    [Fact]
    public async Task ProcessAsync_WithNoCredential_ShouldFail()
    {
        // Arrange
        var validationRules = new List<ValidationRule>
        {
            new ValidationRule { Type = "Required", Configuration = "credentialSubject.id" }
        };

        var input = new W3cValidationAction
        {
            CredentialReference = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "credential"
            },
            ValidationRules = validationRules
        };

        var action = new Domain.ProcessFlow.Actions.Action
        {
            Type = EActionType.W3cValidation,
            Input = input,
            RunAfter = new List<Guid>()
        };

        // Set up the mock context to return null for parameter resolution
        _processingContextMock.Setup(ctx => ctx.ExecutionContext.InputContext)
            .Returns((IReadOnlyDictionary<string, string>)null);

        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContextMock.Object);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _actionOutcome.ActionId.Should().Be(_actionId);
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
        _actionOutcome.ErrorJson.Should().Be("No credential found in the execution context to validate.");
    }

    [Fact]
    public async Task ProcessAsync_WithJsonCredential_ShouldSucceed()
    {
        // Arrange
        var validationRules = new List<ValidationRule>
        {
            new ValidationRule { Type = "Required", Configuration = "credentialSubject.id" }
        };

        var input = new W3cValidationAction
        {
            CredentialReference = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = ValidJsonCredential
            },
            ValidationRules = validationRules
        };

        var action = new Domain.ProcessFlow.Actions.Action
        {
            Type = EActionType.W3cValidation,
            Input = input,
            RunAfter = new List<Guid>()
        };

        var validationResponse = new W3cValidationResponse { IsValid = true };
        _mediatorMock.Setup(m => m.Send(It.IsAny<W3cValidationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(validationResponse));

        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContextMock.Object);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _actionOutcome.ActionId.Should().Be(_actionId);
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Success);
    }

    [Fact]
    public async Task ProcessAsync_WithValidationHandlerError_ShouldFail()
    {
        // Arrange
        var validationRules = new List<ValidationRule>
        {
            new ValidationRule { Type = "Required", Configuration = "credentialSubject.id" }
        };

        var input = new W3cValidationAction
        {
            CredentialReference = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = ValidJwtCredential
            },
            ValidationRules = validationRules
        };

        var action = new Domain.ProcessFlow.Actions.Action
        {
            Type = EActionType.W3cValidation,
            Input = input,
            RunAfter = new List<Guid>()
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<W3cValidationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Error during validation"));

        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContextMock.Object);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _actionOutcome.ActionId.Should().Be(_actionId);
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
        _actionOutcome.ErrorJson.Should().Be("Error during validation");
    }

    [Fact]
    public async Task ProcessAsync_WithCredentialFromActionOutcome_ShouldSucceed()
    {
        // Arrange
        var validationRules = new List<ValidationRule>
        {
            new ValidationRule { Type = "Required", Configuration = "credentialSubject.id" }
        };

        var previousActionId = Guid.NewGuid();
        var input = new W3cValidationAction
        {
            CredentialReference = new ParameterReference
            {
                Source = ParameterSource.ActionOutcome,
                ActionId = previousActionId
            },
            ValidationRules = validationRules
        };

        var action = new Domain.ProcessFlow.Actions.Action
        {
            Type = EActionType.W3cValidation,
            Input = input,
            RunAfter = new List<Guid> { previousActionId }
        };
            
        var previousOutcome = new ActionOutcome(previousActionId);
        previousOutcome.FinishOutcomeWithSuccess(ValidJwtCredential);
        var actionOutcomes = new List<ActionOutcome> { previousOutcome };
            
        // Set up a new mock with the action outcomes
        var processingContextWithOutcome = new Mock<ActionProcessingContext>(
            Mock.Of<object>(),
            actionOutcomes,
            Mock.Of<Domain.Workflow.Workflow>(), 
            CancellationToken.None);
            
        processingContextWithOutcome.Setup(ctx => ctx.ActionOutcomes)
            .Returns(actionOutcomes);

        var validationResponse = new W3cValidationResponse { IsValid = true };
        _mediatorMock.Setup(m => m.Send(It.IsAny<W3cValidationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(validationResponse));

        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, processingContextWithOutcome.Object);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _actionOutcome.ActionId.Should().Be(_actionId);
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Success);
    }

    [Fact]
    public async Task ProcessAsync_WithDifferentRuleTypes_ShouldPassCorrectRulesToHandler()
    {
        // Arrange
        var validationRules = new List<ValidationRule>
        {
            new() { Type = "Required", Configuration = "credentialSubject.id" },
            new() { Type = "Format", Configuration = "issuanceDate:ISO8601" },
            new() { Type = "Range", Configuration = "credentialSubject.age:18-100" }
        };

        var input = new W3cValidationAction
        {
            CredentialReference = new ParameterReference
            {
                Source = ParameterSource.Static,
                
                Path = ValidJwtCredential
            },
            ValidationRules = validationRules
        };

        var action = new Domain.ProcessFlow.Actions.Action
        {
            Type = EActionType.W3cValidation,
            Input = input,
            RunAfter = new List<Guid>()
        };

        W3cValidationRequest capturedRequest = null;
        var validationResponse = new W3cValidationResponse { IsValid = true };
            
        _mediatorMock.Setup(m => m.Send(It.IsAny<W3cValidationRequest>(), It.IsAny<CancellationToken>()))
            .Callback<W3cValidationRequest, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(Result.Ok(validationResponse));

        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContextMock.Object);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedRequest.Should().NotBeNull();
        capturedRequest.Credential.Should().Be(ValidJwtCredential);
        capturedRequest.Rules.Should().HaveCount(3);
        capturedRequest.Rules.Should().Contain(r => r.Type == "Required" && r.Configuration == "credentialSubject.id");
        capturedRequest.Rules.Should().Contain(r => r.Type == "Format" && r.Configuration == "issuanceDate:ISO8601");
        capturedRequest.Rules.Should().Contain(r => r.Type == "Range" && r.Configuration == "credentialSubject.age:18-100");
    }
}