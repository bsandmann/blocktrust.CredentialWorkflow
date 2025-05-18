using Blocktrust.CredentialWorkflow.Core.Commands.ValidateCredentials.CustomValidation;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Validation;
using FluentAssertions;
using System.Text.Json;

public class CustomValidationHandlerTests
{
    private readonly CustomValidationHandler _handler;
    
    // Sample data JSON for testing
    private const string ValidPersonDataJson = @"{
        ""person"": {
            ""name"": ""John Doe"",
            ""age"": 30,
            ""email"": ""john.doe@example.com"",
            ""address"": {
                ""city"": ""New York"",
                ""country"": ""USA"",
                ""zipCode"": ""10001""
            }
        },
        ""isActive"": true,
        ""registrationDate"": ""2023-01-01T12:00:00Z""
    }";
    
    private const string InvalidPersonDataJson = @"{
        ""person"": {
            ""name"": ""Jo"",
            ""age"": 16,
            ""email"": ""not-an-email""
        },
        ""isActive"": false
    }";
    
    private const string MissingFieldsDataJson = @"{
        ""person"": {
            ""name"": ""John Doe""
        }
    }";
    
    private const string ComplexNestedDataJson = @"{
        ""organization"": {
            ""name"": ""Acme Corp"",
            ""founded"": 1985,
            ""address"": {
                ""street"": ""123 Business Ave"",
                ""city"": ""Enterprise"",
                ""country"": ""USA""
            },
            ""departments"": [
                {
                    ""name"": ""Engineering"",
                    ""employeeCount"": 50,
                    ""teams"": [""Frontend"", ""Backend"", ""QA""]
                },
                {
                    ""name"": ""Marketing"",
                    ""employeeCount"": 20,
                    ""teams"": [""Design"", ""Content"", ""Social Media""]
                }
            ],
            ""active"": true
        },
        ""metadata"": {
            ""lastUpdated"": ""2023-06-15T09:30:00Z"",
            ""version"": 2.5
        }
    }";
    
    public CustomValidationHandlerTests()
    {
        _handler = new CustomValidationHandler();
    }
    
    [Fact]
    public async Task Handle_ValidData_NoRules_ShouldReturnValid()
    {
        // Arrange
        var data = JsonSerializer.Deserialize<JsonElement>(ValidPersonDataJson);
        var request = new CustomValidationRequest(data, new List<CustomValidationRule>());
        
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
        
        // Act & Assert
        await Assert.ThrowsAsync<JsonException>(() => 
        {
            var data = JsonSerializer.Deserialize<JsonElement>(invalidJson);
            var request = new CustomValidationRequest(data, new List<CustomValidationRule>());
            return _handler.Handle(request, CancellationToken.None);
        });
    }
    
    [Fact]
    public async Task Handle_SimpleRule_Valid_ShouldBeValid()
    {
        // Arrange
        var data = JsonSerializer.Deserialize<JsonElement>(ValidPersonDataJson);
        var rules = new List<CustomValidationRule>
        {
            new CustomValidationRule
            {
                Name = "NameRule",
                Expression = "data.person.name && data.person.name.length > 2",
                ErrorMessage = "Name is required and must be at least 3 characters"
            }
        };
        
        var request = new CustomValidationRequest(data, rules);
        
        // Act
        var result = await _handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeTrue();
        result.Value.Errors.Should().BeEmpty();
    }
    
    [Fact]
    public async Task Handle_SimpleRule_Invalid_ShouldBeInvalid()
    {
        // Arrange
        var data = JsonSerializer.Deserialize<JsonElement>(InvalidPersonDataJson);
        var rules = new List<CustomValidationRule>
        {
            new CustomValidationRule
            {
                Name = "NameRule",
                Expression = "data.person.name && data.person.name.length > 2",
                ErrorMessage = "Name is required and must be at least 3 characters"
            }
        };
        
        var request = new CustomValidationRequest(data, rules);
        
        // Act
        var result = await _handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeFalse();
        result.Value.Errors.Should().ContainSingle()
            .Which.Message.Should().Be("Name is required and must be at least 3 characters");
    }
    
    [Fact]
    public async Task Handle_NumberComparison_Valid_ShouldBeValid()
    {
        // Arrange
        var data = JsonSerializer.Deserialize<JsonElement>(ValidPersonDataJson);
        var rules = new List<CustomValidationRule>
        {
            new CustomValidationRule
            {
                Name = "AgeRule",
                Expression = "data.person.age >= 18",
                ErrorMessage = "Age must be 18 or above"
            }
        };
        
        var request = new CustomValidationRequest(data, rules);
        
        // Act
        var result = await _handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeTrue();
        result.Value.Errors.Should().BeEmpty();
    }
    
    [Fact]
    public async Task Handle_NumberComparison_Invalid_ShouldBeInvalid()
    {
        // Arrange
        var data = JsonSerializer.Deserialize<JsonElement>(InvalidPersonDataJson);
        var rules = new List<CustomValidationRule>
        {
            new CustomValidationRule
            {
                Name = "AgeRule",
                Expression = "data.person.age >= 18",
                ErrorMessage = "Age must be 18 or above"
            }
        };
        
        var request = new CustomValidationRequest(data, rules);
        
        // Act
        var result = await _handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeFalse();
        result.Value.Errors.Should().ContainSingle()
            .Which.Message.Should().Be("Age must be 18 or above");
    }
    
    [Fact]
    public async Task Handle_EmailFormat_Valid_ShouldBeValid()
    {
        // Arrange
        var data = JsonSerializer.Deserialize<JsonElement>(ValidPersonDataJson);
        var rules = new List<CustomValidationRule>
        {
            new CustomValidationRule
            {
                Name = "EmailRule",
                Expression = "data.person.email && data.person.email.indexOf('@') > 0",
                ErrorMessage = "Email must be valid"
            }
        };
        
        var request = new CustomValidationRequest(data, rules);
        
        // Act
        var result = await _handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeTrue();
        result.Value.Errors.Should().BeEmpty();
    }
    
    [Fact]
    public async Task Handle_EmailFormat_Invalid_ShouldBeInvalid()
    {
        // Arrange
        var data = JsonSerializer.Deserialize<JsonElement>(InvalidPersonDataJson);
        var rules = new List<CustomValidationRule>
        {
            new CustomValidationRule
            {
                Name = "EmailRule",
                Expression = "data.person.email && data.person.email.indexOf('@') > 0",
                ErrorMessage = "Email must be valid"
            }
        };
        
        var request = new CustomValidationRequest(data, rules);
        
        // Act
        var result = await _handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeFalse();
        result.Value.Errors.Should().ContainSingle()
            .Which.Message.Should().Be("Email must be valid");
    }
    
    [Fact]
    public async Task Handle_MissingField_ShouldReportError()
    {
        // Arrange
        var data = JsonSerializer.Deserialize<JsonElement>(MissingFieldsDataJson);
        var rules = new List<CustomValidationRule>
        {
            new CustomValidationRule
            {
                Name = "AgeRule",
                Expression = "data.person.age >= 18",
                ErrorMessage = "Age must be 18 or above"
            }
        };
        
        var request = new CustomValidationRequest(data, rules);
        
        // Act
        var result = await _handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeFalse();
        result.Value.Errors.Should().ContainSingle();
    }
    
    [Fact]
    public async Task Handle_MalformedExpression_ShouldCaptureError()
    {
        // Arrange
        var data = JsonSerializer.Deserialize<JsonElement>(ValidPersonDataJson);
        var rules = new List<CustomValidationRule>
        {
            new CustomValidationRule
            {
                Name = "InvalidSyntaxRule",
                Expression = "data.person.name === 'John' &&& data.person.age > 20", // invalid syntax
                ErrorMessage = "This rule has syntax errors"
            }
        };
        
        var request = new CustomValidationRequest(data, rules);
        
        // Act
        var result = await _handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeFalse();
        result.Value.Errors.Should().ContainSingle();
        result.Value.Errors.First().Message.Should().Contain("Expression evaluation failed");
    }
    
    [Fact]
    public async Task Handle_ComplexNestedObjects_ShouldValidateCorrectly()
    {
        // Arrange
        var data = JsonSerializer.Deserialize<JsonElement>(ComplexNestedDataJson);
        var rules = new List<CustomValidationRule>
        {
            new CustomValidationRule
            {
                Name = "OrganizationNameRule",
                Expression = "data.organization.name.length > 3",
                ErrorMessage = "Organization name must be longer than 3 characters"
            },
            new CustomValidationRule
            {
                Name = "DepartmentCountRule",
                Expression = "data.organization.departments.length >= 2",
                ErrorMessage = "Organization must have at least 2 departments"
            },
            new CustomValidationRule
            {
                Name = "EngineeringTeamRule",
                Expression = "data.organization.departments[0].teams.includes('Frontend')",
                ErrorMessage = "Engineering department must have a Frontend team"
            }
        };
        
        var request = new CustomValidationRequest(data, rules);
        
        // Act
        var result = await _handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeTrue();
        result.Value.Errors.Should().BeEmpty();
    }
    
    [Fact]
    public async Task Handle_DateComparison_ShouldValidateCorrectly()
    {
        // Arrange
        var data = JsonSerializer.Deserialize<JsonElement>(ValidPersonDataJson);
        var rules = new List<CustomValidationRule>
        {
            new CustomValidationRule
            {
                Name = "RegistrationDateRule",
                Expression = "new Date(data.registrationDate) <= new Date()",
                ErrorMessage = "Registration date cannot be in the future"
            }
        };
        
        var request = new CustomValidationRequest(data, rules);
        
        // Act
        var result = await _handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeTrue();
        result.Value.Errors.Should().BeEmpty();
    }
    
    [Fact]
    public async Task Handle_MultipleRules_AllValid_ShouldBeValid()
    {
        // Arrange
        var data = JsonSerializer.Deserialize<JsonElement>(ValidPersonDataJson);
        var rules = new List<CustomValidationRule>
        {
            new CustomValidationRule
            {
                Name = "NameRule",
                Expression = "data.person.name && data.person.name.length > 2", 
                ErrorMessage = "Name is required and must be at least 3 characters"
            },
            new CustomValidationRule
            {
                Name = "AgeRule",
                Expression = "data.person.age >= 18",
                ErrorMessage = "Age must be 18 or above"
            },
            new CustomValidationRule
            {
                Name = "EmailRule", 
                Expression = "data.person.email && data.person.email.indexOf('@') > 0",
                ErrorMessage = "Email must be valid"
            },
            new CustomValidationRule
            {
                Name = "ActiveRule",
                Expression = "data.isActive === true",
                ErrorMessage = "Person must be active"
            }
        };
        
        var request = new CustomValidationRequest(data, rules);
        
        // Act
        var result = await _handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeTrue();
        result.Value.Errors.Should().BeEmpty();
    }
    
    [Fact]
    public async Task Handle_MultipleRules_SomeInvalid_ShouldBeInvalid()
    {
        // Arrange
        var data = JsonSerializer.Deserialize<JsonElement>(InvalidPersonDataJson);
        var rules = new List<CustomValidationRule>
        {
            new CustomValidationRule
            {
                Name = "NameRule",
                Expression = "data.person.name && data.person.name.length > 2", 
                ErrorMessage = "Name is required and must be at least 3 characters"
            },
            new CustomValidationRule
            {
                Name = "AgeRule",
                Expression = "data.person.age >= 18",
                ErrorMessage = "Age must be 18 or above"
            },
            new CustomValidationRule
            {
                Name = "EmailRule", 
                Expression = "data.person.email && data.person.email.indexOf('@') > 0",
                ErrorMessage = "Email must be valid"
            },
            new CustomValidationRule
            {
                Name = "ActiveRule",
                Expression = "data.isActive === true",
                ErrorMessage = "Person must be active"
            }
        };
        
        var request = new CustomValidationRequest(data, rules);
        
        // Act
        var result = await _handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeFalse();
        result.Value.Errors.Should().HaveCount(4);
        result.Value.Errors.Should().Contain(e => e.RuleName == "NameRule");
        result.Value.Errors.Should().Contain(e => e.RuleName == "AgeRule");
        result.Value.Errors.Should().Contain(e => e.RuleName == "EmailRule");
        result.Value.Errors.Should().Contain(e => e.RuleName == "ActiveRule");
    }
    
    [Fact]
    public async Task Handle_NullData_ShouldReturnFailure()
    {
        // Arrange
        object data = null;
        var rules = new List<CustomValidationRule>
        {
            new CustomValidationRule
            {
                Name = "AnyDataRule",
                Expression = "data !== null",
                ErrorMessage = "Data cannot be null"
            }
        };
        
        var request = new CustomValidationRequest(data, rules);
        
        // Act
        var result = await _handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Contain("Validation failed");
    }
    
    [Fact]
    public async Task Handle_ArrayOperations_ShouldValidateCorrectly()
    {
        // Arrange
        var arrayDataJson = @"{
            ""items"": [
                { ""id"": 1, ""name"": ""Item 1"", ""price"": 10.99 },
                { ""id"": 2, ""name"": ""Item 2"", ""price"": 20.50 },
                { ""id"": 3, ""name"": ""Item 3"", ""price"": 5.75 }
            ],
            ""tags"": [""sale"", ""featured"", ""new""]
        }";
        
        var data = JsonSerializer.Deserialize<JsonElement>(arrayDataJson);
        var rules = new List<CustomValidationRule>
        {
            new CustomValidationRule
            {
                Name = "ItemCountRule",
                Expression = "data.items.length > 2",
                ErrorMessage = "Must have more than 2 items"
            },
            new CustomValidationRule
            {
                Name = "TagsContainRule",
                Expression = "data.tags.includes('featured')",
                ErrorMessage = "Must have 'featured' tag"
            },
            new CustomValidationRule
            {
                Name = "PriceCheckRule",
                Expression = "data.items.some(item => item.price < 6.00)",
                ErrorMessage = "Must have at least one item under $6.00"
            }
        };
        
        var request = new CustomValidationRequest(data, rules);
        
        // Act
        var result = await _handler.Handle(request, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeTrue();
        result.Value.Errors.Should().BeEmpty();
    }
}