using Blocktrust.CredentialWorkflow.Core.Commands.ValidateCredentials.CustomValidation;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow.ActionProcessors;
using Blocktrust.CredentialWorkflow.Core.Domain.Common;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Validation;
using FluentAssertions;
using FluentResults;
using MediatR;
using Moq;
using Action = Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Action;
using ExecutionContext = Blocktrust.CredentialWorkflow.Core.Domain.Common.ExecutionContext;


public class CustomValidationProcessorTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly CustomValidationProcessor _processor;
    private readonly Guid _actionId;
    private readonly ActionOutcome _actionOutcome;
    private readonly ActionProcessingContext _processingContext;

    public CustomValidationProcessorTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _processor = new CustomValidationProcessor(_mediatorMock.Object);
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
    public void ActionType_ShouldBeCustomValidation()
    {
        // Assert
        _processor.ActionType.Should().Be(EActionType.CustomValidation);
    }

    [Fact]
    public async Task ProcessAsync_WithValidData_ShouldSucceed()
    {
        // Arrange
        var validationRules = new List<CustomValidationRule>
        {
            new CustomValidationRule
            {
                Name = "AgeRule",
                Expression = "data.person.age >= 18",
                ErrorMessage = "Age must be 18 or above"
            }
        };

        var jsonData = @"{
            ""person"": {
                ""name"": ""John Doe"",
                ""age"": 30
            }
        }";

        var input = new CustomValidationAction
        {
            Id = Guid.NewGuid(),
            DataReference = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = jsonData
            },
            ValidationRules = validationRules
        };

        var action = new Action
        {
            Type = EActionType.CustomValidation,
            Input = input,
            RunAfter = new List<Guid>()
        };

        var validationResponse = new CustomValidationResult { IsValid = true };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<CustomValidationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(validationResponse));

        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Success);
        _actionOutcome.ErrorJson.Should().BeNull();
    }

    [Fact]
    public async Task ProcessAsync_WithInvalidData_ShouldFail()
    {
        // Arrange
        var validationRules = new List<CustomValidationRule>
        {
            new CustomValidationRule
            {
                Name = "AgeRule",
                Expression = "data.person.age >= 18",
                ErrorMessage = "Age must be 18 or above"
            }
        };

        var jsonData = @"{
            ""person"": {
                ""name"": ""John Doe"",
                ""age"": 16
            }
        }";

        var input = new CustomValidationAction
        {
            Id = Guid.NewGuid(),
            DataReference = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = jsonData
            },
            ValidationRules = validationRules
        };

        var action = new Action
        {
            Type = EActionType.CustomValidation,
            Input = input,
            RunAfter = new List<Guid>()
        };

        var errors = new List<CustomValidationError>
        {
            new CustomValidationError("AgeRule", "Age must be 18 or above")
        };

        var validationResponse = new CustomValidationResult
        {
            IsValid = false,
            Errors = errors
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<CustomValidationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(validationResponse));

        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
        _actionOutcome.ErrorJson.Should().NotBeNull();
        _actionOutcome.ErrorJson.Should().Contain("AgeRule: Age must be 18 or above");
    }

    [Fact]
    public async Task ProcessAsync_WithNoData_ShouldFail()
    {
        // Arrange
        var validationRules = new List<CustomValidationRule>
        {
            new CustomValidationRule
            {
                Name = "AgeRule",
                Expression = "data.person.age >= 18",
                ErrorMessage = "Age must be 18 or above"
            }
        };

        var input = new CustomValidationAction
        {
            Id = Guid.NewGuid(),
            DataReference = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "data"
            },
            ValidationRules = validationRules
        };

        var action = new Action
        {
            Type = EActionType.CustomValidation,
            Input = input,
            RunAfter = new List<Guid>()
        };

        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
        _actionOutcome.ErrorJson.Should().Be("No data found in the execution context to validate.");
    }

    [Fact]
    public async Task ProcessAsync_WithMultipleValidationRules_ShouldPassAllRulesToHandler()
    {
        // Arrange
        var validationRules = new List<CustomValidationRule>
        {
            new CustomValidationRule
            {
                Name = "AgeRule",
                Expression = "data.person.age >= 18",
                ErrorMessage = "Age must be 18 or above"
            },
            new CustomValidationRule
            {
                Name = "NameRule",
                Expression = "data.person.name && data.person.name.length > 2",
                ErrorMessage = "Name must be at least 3 characters"
            }
        };

        var jsonData = @"{
            ""person"": {
                ""name"": ""John Doe"",
                ""age"": 30
            }
        }";

        var input = new CustomValidationAction
        {
            Id = Guid.NewGuid(),
            DataReference = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = jsonData
            },
            ValidationRules = validationRules
        };

        var action = new Action
        {
            Type = EActionType.CustomValidation,
            Input = input,
            RunAfter = new List<Guid>()
        };

        CustomValidationRequest capturedRequest = null;
        var validationResponse = new CustomValidationResult { IsValid = true };

        
        _mediatorMock.Setup(m => m.Send(It.IsAny<CustomValidationRequest>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result<CustomValidationResult>>, CancellationToken>((req, _) => capturedRequest = (CustomValidationRequest)req)
            .ReturnsAsync(Result.Ok(validationResponse));

        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedRequest.Should().NotBeNull();
        
        // Verify data was correctly parsed
        capturedRequest.Data.Should().NotBeNull();
        
        // Verify rules were passed correctly
        capturedRequest.Rules.Should().HaveCount(2);
        capturedRequest.Rules.Should().Contain(r => r.Name == "AgeRule" && r.Expression == "data.person.age >= 18");
        capturedRequest.Rules.Should().Contain(r => r.Name == "NameRule" && r.Expression == "data.person.name && data.person.name.length > 2");
    }

    [Fact]
    public async Task ProcessAsync_WithValidationHandlerError_ShouldFail()
    {
        // Arrange
        var validationRules = new List<CustomValidationRule>
        {
            new CustomValidationRule
            {
                Name = "AgeRule",
                Expression = "data.person.age >= 18",
                ErrorMessage = "Age must be 18 or above"
            }
        };

        var jsonData = @"{
            ""person"": {
                ""name"": ""John Doe"",
                ""age"": 30
            }
        }";

        var input = new CustomValidationAction
        {
            Id = Guid.NewGuid(),
            DataReference = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = jsonData
            },
            ValidationRules = validationRules
        };

        var action = new Action
        {
            Type = EActionType.CustomValidation,
            Input = input,
            RunAfter = new List<Guid>()
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<CustomValidationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Error evaluating validation rules"));

        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
        _actionOutcome.ErrorJson.Should().Be("Error evaluating validation rules");
    }

    [Fact]
    public async Task ProcessAsync_WithDataFromActionOutcome_ShouldSucceed()
    {
        // Arrange
        var validationRules = new List<CustomValidationRule>
        {
            new CustomValidationRule
            {
                Name = "AgeRule",
                Expression = "data.person.age >= 18",
                ErrorMessage = "Age must be 18 or above"
            }
        };

        var previousActionId = Guid.NewGuid();
        var jsonData = @"{
            ""person"": {
                ""name"": ""John Doe"",
                ""age"": 30
            }
        }";

        var input = new CustomValidationAction
        {
            Id = Guid.NewGuid(),
            DataReference = new ParameterReference
            {
                Source = ParameterSource.ActionOutcome,
                ActionId = previousActionId
            },
            ValidationRules = validationRules
        };

        var action = new Action
        {
            Type = EActionType.CustomValidation,
            Input = input,
            RunAfter = new List<Guid> { previousActionId }
        };

        var previousOutcome = new ActionOutcome(previousActionId);
        previousOutcome.FinishOutcomeWithSuccess(jsonData);

        var executionContext = new ExecutionContext(Guid.NewGuid());
        var customContext = new ActionProcessingContext(
            executionContext,
            new List<ActionOutcome> { previousOutcome },
            null,
            CancellationToken.None
        );

        var validationResponse = new CustomValidationResult { IsValid = true };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<CustomValidationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(validationResponse));

        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, customContext);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Success);
    }

    [Fact]
    public async Task ProcessAsync_WithMultipleFailingRules_ShouldReportAllErrors()
    {
        // Arrange
        var validationRules = new List<CustomValidationRule>
        {
            new CustomValidationRule
            {
                Name = "AgeRule",
                Expression = "data.person.age >= 18",
                ErrorMessage = "Age must be 18 or above"
            },
            new CustomValidationRule
            {
                Name = "NameRule",
                Expression = "data.person.name && data.person.name.length > 5",
                ErrorMessage = "Name must be at least 6 characters"
            }
        };

        var jsonData = @"{
            ""person"": {
                ""name"": ""John"",
                ""age"": 16
            }
        }";

        var input = new CustomValidationAction
        {
            Id = Guid.NewGuid(),
            DataReference = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = jsonData
            },
            ValidationRules = validationRules
        };

        var action = new Action
        {
            Type = EActionType.CustomValidation,
            Input = input,
            RunAfter = new List<Guid>()
        };

        var errors = new List<CustomValidationError>
        {
            new CustomValidationError("AgeRule", "Age must be 18 or above"),
            new CustomValidationError("NameRule", "Name must be at least 6 characters")
        };

        var validationResponse = new CustomValidationResult
        {
            IsValid = false,
            Errors = errors
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<CustomValidationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(validationResponse));

        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
        _actionOutcome.ErrorJson.Should().NotBeNull();
        _actionOutcome.ErrorJson.Should().Contain("AgeRule: Age must be 18 or above");
        _actionOutcome.ErrorJson.Should().Contain("NameRule: Name must be at least 6 characters");
    }

    [Fact]
    public async Task ProcessAsync_WithInvalidJsonData_ShouldFail()
    {
        // Arrange
        var validationRules = new List<CustomValidationRule>
        {
            new CustomValidationRule
            {
                Name = "AgeRule",
                Expression = "data.person.age >= 18",
                ErrorMessage = "Age must be 18 or above"
            }
        };

        var invalidJsonData = "{ invalid json }";

        var input = new CustomValidationAction
        {
            Id = Guid.NewGuid(),
            DataReference = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = invalidJsonData
            },
            ValidationRules = validationRules
        };

        var action = new Action
        {
            Type = EActionType.CustomValidation,
            Input = input,
            RunAfter = new List<Guid>()
        };

        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
        _actionOutcome.ErrorJson.Should().Contain("No data found in the execution context to validate");
    }

    [Fact]
    public async Task ProcessAsync_WithComplexNestedData_ShouldSucceed()
    {
        // Arrange
        var validationRules = new List<CustomValidationRule>
        {
            new CustomValidationRule
            {
                Name = "OrganizationRule",
                Expression = "data.organization.departments.length >= 2",
                ErrorMessage = "Organization must have at least 2 departments"
            }
        };

        var jsonData = @"{
            ""organization"": {
                ""name"": ""Acme Corp"",
                ""departments"": [
                    {
                        ""name"": ""Engineering"",
                        ""teams"": [""Frontend"", ""Backend"", ""QA""]
                    },
                    {
                        ""name"": ""Marketing"",
                        ""teams"": [""Design"", ""Content""]
                    }
                ]
            }
        }";

        var input = new CustomValidationAction
        {
            Id = Guid.NewGuid(),
            DataReference = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = jsonData
            },
            ValidationRules = validationRules
        };

        var action = new Action
        {
            Type = EActionType.CustomValidation,
            Input = input,
            RunAfter = new List<Guid>()
        };

        var validationResponse = new CustomValidationResult { IsValid = true };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<CustomValidationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(validationResponse));

        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Success);
    }
}