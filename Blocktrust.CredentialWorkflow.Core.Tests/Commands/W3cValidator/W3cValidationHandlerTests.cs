using Blocktrust.CredentialWorkflow.Core.Commands.ValidateCredentials.W3cValidation;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Validation;
using Blocktrust.CredentialWorkflow.Core.Services;
using FluentAssertions;

public class W3cValidationHandlerTests
{
    private readonly W3cValidationHandler _handler;
    private readonly CredentialParser _credentialParser;
    
    // Sample credential JSON
    private const string ValidCredentialJson = @"{
        ""@context"": [""https://www.w3.org/2018/credentials/v1""],
        ""type"": [""VerifiableCredential""],
        ""credentialSubject"": {
            ""id"": ""did:prism:test123"",
            ""name"": ""John Doe"",
            ""age"": 25,
            ""email"": ""john.doe@example.com"",
            ""url"": ""https://example.com"",
            ""did"": ""did:prism:subject123""
        },
        ""issuanceDate"": ""2023-01-01T12:00:00Z""
    }";
    
    private const string InvalidDateCredentialJson = @"{
        ""@context"": [""https://www.w3.org/2018/credentials/v1""],
        ""type"": [""VerifiableCredential""],
        ""credentialSubject"": {
            ""id"": ""did:prism:test123""
        },
        ""issuanceDate"": ""invalid-date""
    }";
    
    private const string InvalidEmailCredentialJson = @"{
        ""@context"": [""https://www.w3.org/2018/credentials/v1""],
        ""type"": [""VerifiableCredential""],
        ""credentialSubject"": {
            ""id"": ""did:prism:test123"",
            ""email"": ""not-an-email""
        },
        ""issuanceDate"": ""2023-01-01T12:00:00Z""
    }";
    
    private const string InvalidAgeCredentialJson = @"{
        ""@context"": [""https://www.w3.org/2018/credentials/v1""],
        ""type"": [""VerifiableCredential""],
        ""credentialSubject"": {
            ""id"": ""did:prism:test123"",
            ""age"": 15
        },
        ""issuanceDate"": ""2023-01-01T12:00:00Z""
    }";
    
    public W3cValidationHandlerTests()
    {
        _credentialParser = new CredentialParser();
        _handler = new W3cValidationHandler(_credentialParser);
    }
    
    [Fact]
    public async Task Handle_ValidCredential_NoRules_ShouldReturnValid()
    {
        // Arrange
        var request = new W3cValidationRequest(ValidCredentialJson, new List<ValidationRule>());
        
        // Act
        var result = await _handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeTrue();
        result.Value.Errors.Should().BeEmpty();
    }
    
    [Fact]
    public async Task Handle_InvalidJson_ShouldReturnFailure()
    {
        // Arrange
        var invalidJson = "{ invalid json }";
        var request = new W3cValidationRequest(invalidJson, new List<ValidationRule>());
        
        // Act
        var result = await _handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Contain("Failed to parse credential");
    }
    
    [Fact]
    public async Task Handle_RequiredField_Present_ShouldBeValid()
    {
        // Arrange
        var rules = new List<ValidationRule>
        {
            new ValidationRule
            {
                Type = "Required",
                Configuration = "credentialSubject.id"
            }
        };
        
        var request = new W3cValidationRequest(ValidCredentialJson, rules);
        
        // Act
        var result = await _handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeTrue();
        result.Value.Errors.Should().BeEmpty();
    }
    
    [Fact]
    public async Task Handle_RequiredField_Missing_ShouldBeInvalid()
    {
        // Arrange
        var rules = new List<ValidationRule>
        {
            new ValidationRule
            {
                Type = "Required",
                Configuration = "credentialSubject.missingField"
            }
        };
        
        var request = new W3cValidationRequest(ValidCredentialJson, rules);
        
        // Act
        var result = await _handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeFalse();
        result.Value.Errors.Should().ContainSingle()
            .Which.Message.Should().Contain("Required field 'credentialSubject.missingField' is missing");
    }
    
    [Fact]
    public async Task Handle_Format_ISO8601_Valid_ShouldBeValid()
    {
        // Arrange
        var rules = new List<ValidationRule>
        {
            new ValidationRule
            {
                Type = "Format",
                Configuration = "issuanceDate:ISO8601"
            }
        };
        
        var request = new W3cValidationRequest(ValidCredentialJson, rules);
        
        // Act
        var result = await _handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeTrue();
        result.Value.Errors.Should().BeEmpty();
    }
    
    [Fact]
    public async Task Handle_Format_ISO8601_Invalid_ShouldBeInvalid()
    {
        // Arrange
        var rules = new List<ValidationRule>
        {
            new ValidationRule
            {
                Type = "Format",
                Configuration = "issuanceDate:ISO8601"
            }
        };
        
        var request = new W3cValidationRequest(InvalidDateCredentialJson, rules);
        
        // Act
        var result = await _handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeFalse();
        result.Value.Errors.Should().ContainSingle()
            .Which.Message.Should().Contain("not a valid ISO8601 date");
    }
    
    [Fact]
    public async Task Handle_Format_Email_Valid_ShouldBeValid()
    {
        // Arrange
        var rules = new List<ValidationRule>
        {
            new ValidationRule
            {
                Type = "Format",
                Configuration = "credentialSubject.email:EMAIL"
            }
        };
        
        var request = new W3cValidationRequest(ValidCredentialJson, rules);
        
        // Act
        var result = await _handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeTrue();
        result.Value.Errors.Should().BeEmpty();
    }
    
    [Fact]
    public async Task Handle_Format_Email_Invalid_ShouldBeInvalid()
    {
        // Arrange
        var rules = new List<ValidationRule>
        {
            new ValidationRule
            {
                Type = "Format",
                Configuration = "credentialSubject.email:EMAIL"
            }
        };
        
        var request = new W3cValidationRequest(InvalidEmailCredentialJson, rules);
        
        // Act
        var result = await _handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeFalse();
        result.Value.Errors.Should().ContainSingle()
            .Which.Message.Should().Contain("not a valid email format");
    }
    
    [Fact]
    public async Task Handle_Range_NumberInRange_ShouldBeValid()
    {
        // Arrange
        var rules = new List<ValidationRule>
        {
            new ValidationRule
            {
                Type = "Range",
                Configuration = "credentialSubject.age:18-65"
            }
        };
        
        var request = new W3cValidationRequest(ValidCredentialJson, rules);
        
        // Act
        var result = await _handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeTrue();
        result.Value.Errors.Should().BeEmpty();
    }
    
    [Fact]
    public async Task Handle_Range_NumberBelowRange_ShouldBeInvalid()
    {
        // Arrange
        var rules = new List<ValidationRule>
        {
            new ValidationRule
            {
                Type = "Range",
                Configuration = "credentialSubject.age:18-65"
            }
        };
        
        var request = new W3cValidationRequest(InvalidAgeCredentialJson, rules);
        
        // Act
        var result = await _handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeFalse();
        result.Value.Errors.Should().ContainSingle()
            .Which.Message.Should().Contain("outside range 18-65");
    }
    
    [Fact]
    public async Task Handle_Multiple_Rules_AllValid_ShouldBeValid()
    {
        // Arrange
        var rules = new List<ValidationRule>
        {
            new ValidationRule { Type = "Required", Configuration = "credentialSubject.id" },
            new ValidationRule { Type = "Required", Configuration = "credentialSubject.name" },
            new ValidationRule { Type = "Format", Configuration = "credentialSubject.email:EMAIL" },
            new ValidationRule { Type = "Range", Configuration = "credentialSubject.age:18-65" }
        };
        
        var request = new W3cValidationRequest(ValidCredentialJson, rules);
        
        // Act
        var result = await _handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeTrue();
        result.Value.Errors.Should().BeEmpty();
    }
    
    [Fact]
    public async Task Handle_Multiple_Rules_SomeInvalid_ShouldBeInvalid()
    {
        // Arrange
        var customCredentialJson = @"{
            ""@context"": [""https://www.w3.org/2018/credentials/v1""],
            ""type"": [""VerifiableCredential""],
            ""credentialSubject"": {
                ""id"": ""did:prism:test123"",
                ""age"": 15,
                ""email"": ""not-an-email""
            },
            ""issuanceDate"": ""2023-01-01T12:00:00Z""
        }";
        
        var rules = new List<ValidationRule>
        {
            new ValidationRule { Type = "Required", Configuration = "credentialSubject.id" },
            new ValidationRule { Type = "Required", Configuration = "credentialSubject.name" },
            new ValidationRule { Type = "Format", Configuration = "credentialSubject.email:EMAIL" },
            new ValidationRule { Type = "Range", Configuration = "credentialSubject.age:18-65" }
        };
        
        var request = new W3cValidationRequest(customCredentialJson, rules);
        
        // Act
        var result = await _handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeFalse();
        result.Value.Errors.Should().HaveCount(3);
        result.Value.Errors.Should().Contain(e => e.RuleType == "Required" && e.Message.Contains("name"));
        result.Value.Errors.Should().Contain(e => e.RuleType == "Format" && e.Message.Contains("email"));
        result.Value.Errors.Should().Contain(e => e.RuleType == "Range" && e.Message.Contains("age"));
    }
}