// using Blocktrust.CredentialWorkflow.Core.Commands.ValidateCredentials.CustomValidation;
// using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow;
// using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow.ActionProcessors;
// using Blocktrust.CredentialWorkflow.Core.Domain.Common;
// using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;
// using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Validation;
// using FluentAssertions;
// using FluentResults;
// using MediatR;
// using Moq;
// using Action = System.Action;
// using ExecutionContext = System.Threading.ExecutionContext;
//
// namespace Blocktrust.CredentialWorkflow.Core.Tests.Commands.Workflow.ExecuteWorkflow.ActionProcessorsTests;
//
// public class CustomValidationProcessorTests
// {
//     private readonly Mock<IMediator> _mediatorMock;
//     private readonly CustomValidationProcessor _processor;
//     private readonly ActionOutcome _actionOutcome;
//     private readonly Guid _actionId;
//     private readonly ActionProcessingContext _processingContext;
//     
//     private const string ValidJsonData = @"{
//         ""person"": {
//             ""name"": ""John Doe"",
//             ""age"": 30,
//             ""email"": ""john.doe@example.com"",
//             ""address"": {
//                 ""street"": ""123 Main St"",
//                 ""city"": ""Anytown"",
//                 ""zipCode"": ""12345""
//             },
//             ""active"": true
//         },
//         ""createdAt"": ""2023-01-01T12:00:00Z"",
//         ""updatedAt"": ""2023-05-15T09:30:00Z""
//     }";
//
//     public CustomValidationProcessorTests()
//     {
//         _mediatorMock = new Mock<IMediator>();
//         _processor = new CustomValidationProcessor(_mediatorMock.Object);
//         _actionId = Guid.NewGuid();
//         _actionOutcome = new ActionOutcome(_actionId);
//         var executionContext = new ExecutionContext(Guid.NewGuid());
//         _processingContext = new ActionProcessingContext(
//             executionContext,
//             new List<ActionOutcome>(),
//             null,
//             CancellationToken.None
//         );
//     }
//
//     [Fact]
//     public void ActionType_ShouldBeCustomValidation()
//     {
//         _processor.ActionType.Should().Be(EActionType.CustomValidation);
//     }
//
//     [Fact]
//     public async Task ProcessAsync_WithValidData_ValidRules_ShouldSucceed()
//     {
//         // Arrange
//         var validationRules = new List<CustomValidationRule>
//         {
//             new CustomValidationRule
//             {
//                 Name = "NameRule",
//                 Expression = "data.person.name != null && data.person.name.length > 2",
//                 ErrorMessage = "Name is required and must be at least 3 characters"
//             },
//             new CustomValidationRule
//             {
//                 Name = "AgeRule",
//                 Expression = "data.person.age >= 18",
//                 ErrorMessage = "Age must be 18 or above"
//             }
//         };
//         
//         var input = new CustomValidationAction
//         {
//             DataReference = new ParameterReference
//             {
//                 Source = ParameterSource.Static,
//                 Path = ValidJsonData
//             },
//             ValidationRules = validationRules
//         };
//         
//         var action = new Action
//         {
//             Type = EActionType.CustomValidation,
//             Input = input,
//             RunAfter = new List<Guid>()
//         };
//         
//         var validationResult = new CustomValidationResult { IsValid = true };
//         _mediatorMock.Setup(m => m.Send(It.IsAny<CustomValidationRequest>(), It.IsAny<CancellationToken>()))
//             .ReturnsAsync(Result.Ok(validationResult));
//         
//         // Act
//         var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);
//         
//         // Assert
//         result.IsSuccess.Should().BeTrue();
//         _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Success);
//         _actionOutcome.ErrorJson.Should().BeNull();
//     }
//
//     [Fact]
//     public async Task ProcessAsync_WithInvalidData_ShouldFail()
//     {
//         // Arrange
//         var validationRules = new List<CustomValidationRule>
//         {
//             new CustomValidationRule
//             {
//                 Name = "NameRule",
//                 Expression = "data.person.name.length > 10", // Will fail
//                 ErrorMessage = "Name must be longer than 10 characters"
//             }
//         };
//         
//         var input = new CustomValidationAction
//         {
//             DataReference = new ParameterReference
//             {
//                 Source = ParameterSource.Static,
//                 Path = ValidJsonData
//             },
//             ValidationRules = validationRules
//         };
//         
//         var action = new Action
//         {
//             Type = EActionType.CustomValidation,
//             Input = input,
//             RunAfter = new List<Guid>()
//         };
//         
//         var errors = new List<CustomValidationError>
//         {
//             new CustomValidationError("NameRule", "Name must be longer than 10 characters")
//         };
//         
//         var validationResult = new CustomValidationResult 
//         { 
//             IsValid = false, 
//             Errors = errors 
//         };
//         
//         _mediatorMock.Setup(m => m.Send(It.IsAny<CustomValidationRequest>(), It.IsAny<CancellationToken>()))
//             .ReturnsAsync(Result.Ok(validationResult));
//         
//         // Act
//         var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);
//         
//         // Assert
//         result.IsSuccess.Should().BeFalse();
//         _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
//         _actionOutcome.ErrorJson.Should().NotBeNull();
//         _actionOutcome.ErrorJson.Should().Contain("NameRule: Name must be longer than 10 characters");
//     }
//
//     [Fact]
//     public async Task ProcessAsync_WithNoData_ShouldFail()
//     {
//         // Arrange
//         var validationRules = new List<CustomValidationRule>
//         {
//             new CustomValidationRule
//             {
//                 Name = "AnyDataRule",
//                 Expression = "data != null",
//                 ErrorMessage = "Data must be provided"
//             }
//         };
//         
//         var input = new CustomValidationAction
//         {
//             DataReference = new ParameterReference
//             {
//                 Source = ParameterSource.TriggerInput,
//                 Path = "data"
//             },
//             ValidationRules = validationRules
//         };
//         
//         var action = new Action
//         {
//             Type = EActionType.CustomValidation,
//             Input = input,
//             RunAfter = new List<Guid>()
//         };
//         
//         // Act
//         var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);
//         
//         // Assert
//         result.IsSuccess.Should().BeFalse();
//         _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
//         _actionOutcome.ErrorJson.Should().Be("No data found in the execution context to validate.");
//     }
//
//     [Fact]
//     public async Task ProcessAsync_WithValidationHandlerError_ShouldFail()
//     {
//         // Arrange
//         var validationRules = new List<CustomValidationRule>
//         {
//             new CustomValidationRule
//             {
//                 Name = "SimpleRule",
//                 Expression = "data.person.active === true",
//                 ErrorMessage = "Person must be active"
//             }
//         };
//         
//         var input = new CustomValidationAction
//         {
//             DataReference = new ParameterReference
//             {
//                 Source = ParameterSource.Static,
//                 Path = ValidJsonData
//             },
//             ValidationRules = validationRules
//         };
//         
//         var action = new Action
//         {
//             Type = EActionType.CustomValidation,
//             Input = input,
//             RunAfter = new List<Guid>()
//         };
//         
//         _mediatorMock.Setup(m => m.Send(It.IsAny<CustomValidationRequest>(), It.IsAny<CancellationToken>()))
//             .ReturnsAsync(Result.Fail("Error during validation"));
//         
//         // Act
//         var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);
//         
//         // Assert
//         result.IsSuccess.Should().BeFalse();
//         _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
//         _actionOutcome.ErrorJson.Should().Be("Error during validation");
//     }
//
//     [Fact]
//     public async Task ProcessAsync_WithDataFromActionOutcome_ShouldSucceed()
//     {
//         // Arrange
//         var validationRules = new List<CustomValidationRule>
//         {
//             new CustomValidationRule
//             {
//                 Name = "SimpleRule",
//                 Expression = "data.person.active === true",
//                 ErrorMessage = "Person must be active"
//             }
//         };
//         
//         var previousActionId = Guid.NewGuid();
//         var input = new CustomValidationAction
//         {
//             DataReference = new ParameterReference
//             {
//                 Source = ParameterSource.ActionOutcome,
//                 ActionId = previousActionId
//             },
//             ValidationRules = validationRules
//         };
//         
//         var action = new Action
//         {
//             Type = EActionType.CustomValidation,
//             Input = input,
//             RunAfter = new List<Guid> { previousActionId }
//         };
//         
//         var previousOutcome = new ActionOutcome(previousActionId);
//         previousOutcome.FinishOutcomeWithSuccess(ValidJsonData);
//         var customContext = new ActionProcessingContext(
//             _processingContext.ExecutionContext,
//             new List<ActionOutcome> { previousOutcome },
//             null,
//             CancellationToken.None
//         );
//         
//         var validationResult = new CustomValidationResult { IsValid = true };
//         _mediatorMock.Setup(m => m.Send(It.IsAny<CustomValidationRequest>(), It.IsAny<CancellationToken>()))
//             .ReturnsAsync(Result.Ok(validationResult));
//         
//         // Act
//         var result = await _processor.ProcessAsync(action, _actionOutcome, customContext);
//         
//         // Assert
//         result.IsSuccess.Should().BeTrue();
//         _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Success);
//     }
//
//     [Fact]
//     public async Task ProcessAsync_WithMultipleValidationRules_ShouldPassAllRulesToHandler()
//     {
//         // Arrange
//         var validationRules = new List<CustomValidationRule>
//         {
//             new CustomValidationRule
//             {
//                 Name = "NameRule",
//                 Expression = "data.person.name != null && data.person.name.length > 2",
//                 ErrorMessage = "Name is required and must be at least 3 characters"
//             },
//             new CustomValidationRule
//             {
//                 Name = "AgeRule",
//                 Expression = "data.person.age >= 18",
//                 ErrorMessage = "Age must be 18 or above"
//             },
//             new CustomValidationRule
//             {
//                 Name = "EmailRule",
//                 Expression = "data.person.email.indexOf('@') > 0",
//                 ErrorMessage = "Email must be valid"
//             }
//         };
//         
//         var input = new CustomValidationAction
//         {
//             DataReference = new ParameterReference
//             {
//                 Source = ParameterSource.Static,
//                 Path = ValidJsonData
//             },
//             ValidationRules = validationRules
//         };
//         
//         var action = new Action
//         {
//             Type = EActionType.CustomValidation,
//             Input = input,
//             RunAfter = new List<Guid>()
//         };
//         
//         CustomValidationRequest capturedRequest = null;
//         var validationResult = new CustomValidationResult { IsValid = true };
//         _mediatorMock.Setup(m => m.Send(It.IsAny<CustomValidationRequest>(), It.IsAny<CancellationToken>()))
//             .Callback<CustomValidationRequest, CancellationToken>((req, _) => capturedRequest = req)
//             .ReturnsAsync(Result.Ok(validationResult));
//         
//         // Act
//         var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);
//         
//         // Assert
//         result.IsSuccess.Should().BeTrue();
//         capturedRequest.Should().NotBeNull();
//         capturedRequest.Rules.Should().HaveCount(3);
//         capturedRequest.Rules.Should().Contain(r => r.Name == "NameRule" && r.Expression.Contains("data.person.name"));
//         capturedRequest.Rules.Should().Contain(r => r.Name == "AgeRule" && r.Expression.Contains("data.person.age"));
//         capturedRequest.Rules.Should().Contain(r => r.Name == "EmailRule" && r.Expression.Contains("data.person.email"));
//     }
//     
//     [Fact]
//     public async Task ProcessAsync_WithPartiallyValidData_ShouldReportAllErrors()
//     {
//         // Arrange
//         var invalidJsonData = @"{
//             ""person"": {
//                 ""name"": ""Jo"",
//                 ""age"": 16,
//                 ""email"": ""invalid-email""
//             }
//         }";
//         
//         var validationRules = new List<CustomValidationRule>
//         {
//             new CustomValidationRule
//             {
//                 Name = "NameRule",
//                 Expression = "data.person.name.length > 2",
//                 ErrorMessage = "Name must be at least 3 characters"
//             },
//             new CustomValidationRule
//             {
//                 Name = "AgeRule",
//                 Expression = "data.person.age >= 18",
//                 ErrorMessage = "Age must be 18 or above"
//             },
//             new CustomValidationRule
//             {
//                 Name = "EmailRule",
//                 Expression = "data.person.email.indexOf('@') > 0",
//                 ErrorMessage = "Email must contain an @ symbol"
//             }
//         };
//         
//         var input = new CustomValidationAction
//         {
//             DataReference = new ParameterReference
//             {
//                 Source = ParameterSource.Static,
//                 Path = invalidJsonData
//             },
//             ValidationRules = validationRules
//         };
//         
//         var action = new Action
//         {
//             Type = EActionType.CustomValidation,
//             Input = input,
//             RunAfter = new List<Guid>()
//         };
//         
//         var errors = new List<CustomValidationError>
//         {
//             new CustomValidationError("NameRule", "Name must be at least 3 characters"),
//             new CustomValidationError("AgeRule", "Age must be 18 or above"),
//             new CustomValidationError("EmailRule", "Email must contain an @ symbol")
//         };
//         
//         var validationResult = new CustomValidationResult 
//         { 
//             IsValid = false, 
//             Errors = errors 
//         };
//         
//         _mediatorMock.Setup(m => m.Send(It.IsAny<CustomValidationRequest>(), It.IsAny<CancellationToken>()))
//             .ReturnsAsync(Result.Ok(validationResult));
//         
//         // Act
//         var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);
//         
//         // Assert
//         result.IsSuccess.Should().BeFalse();
//         _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
//         _actionOutcome.ErrorJson.Should().NotBeNull();
//         _actionOutcome.ErrorJson.Should().Contain("NameRule: Name must be at least 3 characters");
//         _actionOutcome.ErrorJson.Should().Contain("AgeRule: Age must be 18 or above");
//         _actionOutcome.ErrorJson.Should().Contain("EmailRule: Email must contain an @ symbol");
//     }
//     
//     [Fact]
//     public async Task ProcessAsync_WithMalformedJson_ShouldFail()
//     {
//         // Arrange
//         var malformedJson = "{ this is not valid json }";
//         
//         var validationRules = new List<CustomValidationRule>
//         {
//             new CustomValidationRule
//             {
//                 Name = "SimpleRule",
//                 Expression = "data != null",
//                 ErrorMessage = "Data must be provided"
//             }
//         };
//         
//         var input = new CustomValidationAction
//         {
//             DataReference = new ParameterReference
//             {
//                 Source = ParameterSource.Static,
//                 Path = malformedJson
//             },
//             ValidationRules = validationRules
//         };
//         
//         var action = new Action
//         {
//             Type = EActionType.CustomValidation,
//             Input = input,
//             RunAfter = new List<Guid>()
//         };
//         
//         _mediatorMock.Setup(m => m.Send(It.IsAny<CustomValidationRequest>(), It.IsAny<CancellationToken>()))
//             .ReturnsAsync(Result.Fail("Failed to parse JSON data"));
//         
//         // Act
//         var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);
//         
//         // Assert
//         result.IsSuccess.Should().BeFalse();
//         _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
//         _actionOutcome.ErrorJson.Should().Be("Failed to parse JSON data");
//     }
//     
//     [Fact]
//     public async Task ProcessAsync_WithTriggerInputDataSource_ShouldRetrieveDataFromContext()
//     {
//         // Arrange
//         var validationRules = new List<CustomValidationRule>
//         {
//             new CustomValidationRule
//             {
//                 Name = "SimpleRule",
//                 Expression = "data.person.active === true",
//                 ErrorMessage = "Person must be active"
//             }
//         };
//         
//         var input = new CustomValidationAction
//         {
//             DataReference = new ParameterReference
//             {
//                 Source = ParameterSource.TriggerInput,
//                 Path = "customerData"
//             },
//             ValidationRules = validationRules
//         };
//         
//         var action = new Action
//         {
//             Type = EActionType.CustomValidation,
//             Input = input,
//             RunAfter = new List<Guid>()
//         };
//         
//         // Create a context with input parameters
//         var executionContext = new ExecutionContext(
//             Guid.NewGuid(),
//             new Dictionary<string, string>
//             {
//                 { "customerdata", ValidJsonData }
//             }
//         );
//         
//         var customContext = new ActionProcessingContext(
//             executionContext,
//             new List<ActionOutcome>(),
//             null,
//             CancellationToken.None
//         );
//         
//         var validationResult = new CustomValidationResult { IsValid = true };
//         _mediatorMock.Setup(m => m.Send(It.IsAny<CustomValidationRequest>(), It.IsAny<CancellationToken>()))
//             .ReturnsAsync(Result.Ok(validationResult));
//         
//         // Act
//         var result = await _processor.ProcessAsync(action, _actionOutcome, customContext);
//         
//         // Assert
//         result.IsSuccess.Should().BeTrue();
//         _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Success);
//     }
//     
//     [Fact]
//     public async Task ProcessAsync_WithSkipToActionId_OnFailure_ShouldSetFailureAction()
//     {
//         // Arrange
//         var validationRules = new List<CustomValidationRule>
//         {
//             new CustomValidationRule
//             {
//                 Name = "NameRule",
//                 Expression = "data.person.name.length > 10", // Will fail
//                 ErrorMessage = "Name must be longer than 10 characters"
//             }
//         };
//         
//         var skipToActionId = Guid.NewGuid();
//         
//         var input = new CustomValidationAction
//         {
//             DataReference = new ParameterReference
//             {
//                 Source = ParameterSource.Static,
//                 Path = ValidJsonData
//             },
//             ValidationRules = validationRules,
//             FailureAction = "Skip",
//             SkipToActionId = skipToActionId
//         };
//         
//         var action = new Action
//         {
//             Type = EActionType.CustomValidation,
//             Input = input,
//             RunAfter = new List<Guid>()
//         };
//         
//         var errors = new List<CustomValidationError>
//         {
//             new CustomValidationError("NameRule", "Name must be longer than 10 characters")
//         };
//         
//         var validationResult = new CustomValidationResult 
//         { 
//             IsValid = false, 
//             Errors = errors 
//         };
//         
//         _mediatorMock.Setup(m => m.Send(It.IsAny<CustomValidationRequest>(), It.IsAny<CancellationToken>()))
//             .ReturnsAsync(Result.Ok(validationResult));
//         
//         // Act
//         var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);
//         
//         // Assert
//         // Note: In a real implementation, this would actually set some "next action" property
//         // in the action outcome or context. This test just verifies the FailureAction property is used.
//         result.IsSuccess.Should().BeFalse();
//         _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
//         _actionOutcome.ErrorJson.Should().Contain("Name must be longer than 10 characters");
//     }
//     
//     [Fact]
//     public async Task ProcessAsync_WithDefaultValue_ShouldUseDefaultWhenDataNotFound()
//     {
//         // Arrange
//         var validationRules = new List<CustomValidationRule>
//         {
//             new CustomValidationRule
//             {
//                 Name = "SimpleRule",
//                 Expression = "data.person.active === true",
//                 ErrorMessage = "Person must be active"
//             }
//         };
//         
//         var input = new CustomValidationAction
//         {
//             DataReference = new ParameterReference
//             {
//                 Source = ParameterSource.TriggerInput,
//                 Path = "nonExistentData",
//                 DefaultValue = ValidJsonData  // Default value to use
//             },
//             ValidationRules = validationRules
//         };
//         
//         var action = new Action
//         {
//             Type = EActionType.CustomValidation,
//             Input = input,
//             RunAfter = new List<Guid>()
//         };
//         
//         var validationResult = new CustomValidationResult { IsValid = true };
//         _mediatorMock.Setup(m => m.Send(It.IsAny<CustomValidationRequest>(), It.IsAny<CancellationToken>()))
//             .ReturnsAsync(Result.Ok(validationResult));
//         
//         // Act
//         var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);
//         
//         // Assert
//         result.IsSuccess.Should().BeTrue();
//         _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Success);
//     }
//     
//     [Fact]
//     public async Task ProcessAsync_WithAppSettings_ShouldRetrieveFromConfiguration()
//     {
//         // Arrange
//         var validationRules = new List<CustomValidationRule>
//         {
//             new CustomValidationRule
//             {
//                 Name = "SimpleRule",
//                 Expression = "data != null",
//                 ErrorMessage = "Data must be provided"
//             }
//         };
//         
//         var input = new CustomValidationAction
//         {
//             DataReference = new ParameterReference
//             {
//                 Source = ParameterSource.AppSettings,
//                 Path = "ValidationDataTemplate"
//             },
//             ValidationRules = validationRules
//         };
//         
//         var action = new Action
//         {
//             Type = EActionType.CustomValidation,
//             Input = input,
//             RunAfter = new List<Guid>()
//         };
//         
//         // Mock the parameter resolution
//         var mockWorkflow = new Mock<Domain.Workflow.Workflow>();
//         var customContext = new ActionProcessingContext(
//             _processingContext.ExecutionContext,
//             _processingContext.ActionOutcomes,
//             mockWorkflow.Object,
//             CancellationToken.None
//         );
//         
//         var validationResult = new CustomValidationResult { IsValid = true };
//         _mediatorMock.Setup(m => m.Send(It.IsAny<CustomValidationRequest>(), It.IsAny<CancellationToken>()))
//             .ReturnsAsync(Result.Ok(validationResult));
//         
//         // Act
//         var result = await _processor.ProcessAsync(action, _actionOutcome, customContext);
//         
//         // Note: In a real scenario, the parameter resolver would get a configuration value
//         // This test assumes the resolver handles that properly
//         
//         // Assert
//         // If parameter resolution fails, this would fail due to no data found
//         result.IsSuccess.Should().BeFalse();
//         _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
//     }
//     
//     [Fact]
//     public async Task ProcessAsync_WithErrorMessageTemplate_ShouldUseTemplate()
//     {
//         // Arrange
//         var validationRules = new List<CustomValidationRule>
//         {
//             new CustomValidationRule
//             {
//                 Name = "NameRule",
//                 Expression = "data.person.name.length > 10", // Will fail
//                 ErrorMessage = "Name must be longer than 10 characters"
//             }
//         };
//         
//         var input = new CustomValidationAction
//         {
//             DataReference = new ParameterReference
//             {
//                 Source = ParameterSource.Static,
//                 Path = ValidJsonData
//             },
//             ValidationRules = validationRules,
//             ErrorMessageTemplate = "Validation failed: [ErrorDetails]"
//         };
//         
//         var action = new Action
//         {
//             Type = EActionType.CustomValidation,
//             Input = input,
//             RunAfter = new List<Guid>()
//         };
//         
//         var errors = new List<CustomValidationError>
//         {
//             new CustomValidationError("NameRule", "Name must be longer than 10 characters")
//         };
//         
//         var validationResult = new CustomValidationResult 
//         { 
//             IsValid = false, 
//             Errors = errors 
//         };
//         
//         _mediatorMock.Setup(m => m.Send(It.IsAny<CustomValidationRequest>(), It.IsAny<CancellationToken>()))
//             .ReturnsAsync(Result.Ok(validationResult));
//         
//         // Act
//         var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);
//         
//         // Assert
//         result.IsSuccess.Should().BeFalse();
//         _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
//         _actionOutcome.ErrorJson.Should().StartWith("Validation failed:");
//         _actionOutcome.ErrorJson.Should().Contain("Name must be longer than 10 characters");
//     }
// }