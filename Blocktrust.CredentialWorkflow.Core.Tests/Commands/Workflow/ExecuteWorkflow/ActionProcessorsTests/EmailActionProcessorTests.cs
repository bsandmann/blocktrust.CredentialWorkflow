using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow.ActionProcessors;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.SendEmailAction;
using Blocktrust.CredentialWorkflow.Core.Domain.Common;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Outgoing;
using FluentAssertions;
using FluentResults;
using MediatR;
using Moq;
using Action = Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Action;
using ExecutionContext = Blocktrust.CredentialWorkflow.Core.Domain.Common.ExecutionContext;


public class EmailActionProcessorTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly EmailActionProcessor _processor;
    private readonly Guid _actionId;
    private readonly ActionOutcome _actionOutcome;
    private readonly ActionProcessingContext _processingContext;

    public EmailActionProcessorTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _processor = new EmailActionProcessor(_mediatorMock.Object);
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
    public void ActionType_ShouldBeEmail()
    {
        // Assert
        _processor.ActionType.Should().Be(EActionType.Email);
    }

    [Fact]
    public async Task ProcessAsync_WithValidInputs_ShouldSendEmail()
    {
        // Arrange
        var input = new EmailAction
        {
            To = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = "user@example.com"
            },
            Subject = "Test Subject {{Name}}",
            Body = "Hello {{Name}}, This is a test email from {{Source}}.",
            Parameters = new Dictionary<string, ParameterReference>
            {
                ["Name"] = new ParameterReference
                {
                    Source = ParameterSource.Static,
                    Path = "John Doe"
                },
                ["Source"] = new ParameterReference
                {
                    Source = ParameterSource.Static,
                    Path = "Test System"
                }
            }
        };

        var action = new Action
        {
            Type = EActionType.Email,
            Input = input,
            RunAfter = new List<Guid>()
        };

        IRequest<Result> capturedRequest = null;
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<SendEmailActionRequest>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result>, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(Result.Ok());

        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Success);
        _actionOutcome.OutcomeJson.Should().Be("Email sent successfully");

        // Verify the email was sent with correct parameters
        capturedRequest.Should().NotBeNull();
        capturedRequest.Should().BeOfType<SendEmailActionRequest>();
        var emailRequest = (SendEmailActionRequest)capturedRequest;
        emailRequest.ToEmail.Should().Be("user@example.com");
        emailRequest.Subject.Should().Be("Test Subject John Doe");
        emailRequest.Body.Should().Be("Hello John Doe, This is a test email from Test System.");
    }

    [Fact]
    public async Task ProcessAsync_WithMissingToEmail_ShouldFail()
    {
        // Arrange
        var input = new EmailAction
        {
            To = new ParameterReference
            {
                Source = ParameterSource.TriggerInput,
                Path = "email"  // Not present in the execution context
            },
            Subject = "Test Subject",
            Body = "Test Body"
        };

        var action = new Action
        {
            Type = EActionType.Email,
            Input = input,
            RunAfter = new List<Guid>()
        };

        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
        _actionOutcome.ErrorJson.Should().Contain("recipient email address is not provided");
            
        // Verify no email was sent
        _mediatorMock.Verify(
            m => m.Send(
                It.IsAny<SendEmailActionRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_WithSendEmailFailure_ShouldReturnError()
    {
        // Arrange
        var input = new EmailAction
        {
            To = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = "user@example.com"
            },
            Subject = "Test Subject",
            Body = "Test Body"
        };

        var action = new Action
        {
            Type = EActionType.Email,
            Input = input,
            RunAfter = new List<Guid>()
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<SendEmailActionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("SMTP server connection failed"));

        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
        _actionOutcome.ErrorJson.Should().Contain("SMTP server connection failed");
    }

    [Fact]
    public async Task ProcessAsync_WithException_ShouldHandleGracefully()
    {
        // Arrange
        var input = new EmailAction
        {
            To = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = "user@example.com"
            },
            Subject = "Test Subject",
            Body = "Test Body"
        };

        var action = new Action
        {
            Type = EActionType.Email,
            Input = input,
            RunAfter = new List<Guid>()
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<SendEmailActionRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _actionOutcome.EActionOutcome.Should().Be(EActionOutcome.Failure);
        _actionOutcome.ErrorJson.Should().Contain("Error sending email: Unexpected error");
    }

    [Fact]
    public async Task ProcessAsync_WithMissingParameters_ShouldStillSendEmail()
    {
        // Arrange
        var input = new EmailAction
        {
            To = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = "user@example.com"
            },
            Subject = "Test Subject {{Name}}",
            Body = "Hello {{Name}}, This is a test email from {{Source}}.",
            Parameters = new Dictionary<string, ParameterReference>
            {
                // Only providing one of the parameters
                ["Source"] = new ParameterReference
                {
                    Source = ParameterSource.Static,
                    Path = "Test System"
                }
            }
        };

        var action = new Action
        {
            Type = EActionType.Email,
            Input = input,
            RunAfter = new List<Guid>()
        };

        IRequest<Result> capturedRequest = null;
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<SendEmailActionRequest>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result>, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(Result.Ok());

        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);

        // Assert
        result.IsSuccess.Should().BeTrue();
            
        // Verify the email was sent with the {{Name}} placeholder remaining
        capturedRequest.Should().NotBeNull();
        capturedRequest.Should().BeOfType<SendEmailActionRequest>();
        var emailRequest = (SendEmailActionRequest)capturedRequest;
        emailRequest.ToEmail.Should().Be("user@example.com");
        emailRequest.Subject.Should().Be("Test Subject {{Name}}");
        emailRequest.Body.Should().Be("Hello {{Name}}, This is a test email from Test System.");
    }

    [Fact]
    public async Task ProcessAsync_WithParameterFromPreviousActionOutcome_ShouldUseValue()
    {
        // Arrange
        var previousActionId = Guid.NewGuid();
        var input = new EmailAction
        {
            To = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = "user@example.com"
            },
            Subject = "Verification Code",
            Body = "Your verification code is {{Code}}",
            Parameters = new Dictionary<string, ParameterReference>
            {
                ["Code"] = new ParameterReference
                {
                    Source = ParameterSource.ActionOutcome,
                    ActionId = previousActionId
                }
            }
        };

        var action = new Action
        {
            Type = EActionType.Email,
            Input = input,
            RunAfter = new List<Guid> { previousActionId }
        };

        // Create a previous action outcome with a result
        var previousOutcome = new ActionOutcome(previousActionId);
        previousOutcome.FinishOutcomeWithSuccess("123456");
            
        var customContext = new ActionProcessingContext(
            _processingContext.ExecutionContext,
            new List<ActionOutcome> { previousOutcome },
            null,
            CancellationToken.None
        );

        IRequest<Result> capturedRequest = null;
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<SendEmailActionRequest>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result>, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(Result.Ok());

        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, customContext);

        // Assert
        result.IsSuccess.Should().BeTrue();
            
        // Verify the email was sent with the correct verification code
        capturedRequest.Should().NotBeNull();
        capturedRequest.Should().BeOfType<SendEmailActionRequest>();
        var emailRequest = (SendEmailActionRequest)capturedRequest;
        emailRequest.ToEmail.Should().Be("user@example.com");
        emailRequest.Subject.Should().Be("Verification Code");
        emailRequest.Body.Should().Be("Your verification code is 123456");
    }

    [Fact]
    public async Task ProcessAsync_WithNullParameter_ShouldKeepPlaceholder()
    {
        // Arrange
        var input = new EmailAction
        {
            To = new ParameterReference
            {
                Source = ParameterSource.Static,
                Path = "user@example.com"
            },
            Subject = "Test Subject",
            Body = "Hello {{Name}}",
            Parameters = new Dictionary<string, ParameterReference>
            {
                ["Name"] = new ParameterReference
                {
                    Source = ParameterSource.TriggerInput,
                    Path = "nullValue" // Will resolve to null
                }
            }
        };

        var action = new Action
        {
            Type = EActionType.Email,
            Input = input,
            RunAfter = new List<Guid>()
        };

        IRequest<Result> capturedRequest = null;
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<SendEmailActionRequest>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result>, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(Result.Ok());

        // Act
        var result = await _processor.ProcessAsync(action, _actionOutcome, _processingContext);

        // Assert
        result.IsSuccess.Should().BeTrue();
    
        // Verify the email was sent with placeholder preserved (since parameter value is null)
        capturedRequest.Should().NotBeNull();
        capturedRequest.Should().BeOfType<SendEmailActionRequest>();
        var emailRequest = (SendEmailActionRequest)capturedRequest;
        emailRequest.ToEmail.Should().Be("user@example.com");
        emailRequest.Subject.Should().Be("Test Subject");
        emailRequest.Body.Should().Be("Hello {{Name}}");
    }
    [Fact]
    public void ProcessEmailTemplate_WithNoParameters_ShouldReturnOriginalTemplate()
    {
        // Arrange
        var template = "This is a test template without parameters.";
            
        // Act
        var result = EmailActionProcessor.ProcessEmailTemplate(template, null);
            
        // Assert
        result.Should().Be(template);
    }

    [Fact]
    public void ProcessEmailTemplate_WithEmptyTemplate_ShouldReturnEmptyString()
    {
        // Arrange
        var parameters = new Dictionary<string, string>
        {
            ["Name"] = "John"
        };
            
        // Act
        var result = EmailActionProcessor.ProcessEmailTemplate("", parameters);
            
        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ProcessEmailTemplate_WithMultipleParameterOccurrences_ShouldReplaceAll()
    {
        // Arrange
        var template = "Hello {{Name}}, your name is {{Name}} and you are {{Age}} years old.";
        var parameters = new Dictionary<string, string>
        {
            ["Name"] = "John",
            ["Age"] = "30"
        };
            
        // Act
        var result = EmailActionProcessor.ProcessEmailTemplate(template, parameters);
            
        // Assert
        result.Should().Be("Hello John, your name is John and you are 30 years old.");
    }

    [Fact]
    public void ProcessEmailTemplate_WithCaseInsensitiveParameters_ShouldReplaceCorrectly()
    {
        // Arrange
        var template = "Hello {{NaMe}}, welcome to {{SyStEm}}!";
        var parameters = new Dictionary<string, string>
        {
            ["name"] = "Alice",
            ["system"] = "Our Platform"
        };
            
        // Act
        var result = EmailActionProcessor.ProcessEmailTemplate(template, parameters);
            
        // Assert
        result.Should().Be("Hello Alice, welcome to Our Platform!");
    }

    [Fact]
    public void ProcessEmailTemplate_WithExtraParameters_ShouldIgnoreUnused()
    {
        // Arrange
        var template = "Hello {{Name}}!";
        var parameters = new Dictionary<string, string>
        {
            ["Name"] = "Bob",
            ["Age"] = "25",
            ["Location"] = "New York"
        };
            
        // Act
        var result = EmailActionProcessor.ProcessEmailTemplate(template, parameters);
            
        // Assert
        result.Should().Be("Hello Bob!");
    }
}