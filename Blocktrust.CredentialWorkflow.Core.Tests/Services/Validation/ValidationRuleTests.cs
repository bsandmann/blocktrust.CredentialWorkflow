namespace Blocktrust.CredentialWorkflow.Core.Tests.Services.Validation;

using System.Text.Json;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Validation;
using Blocktrust.CredentialWorkflow.Core.Services;
using FluentAssertions;
using Xunit;

public class ValidationRuleTests
{
    #region Required Field Rules Tests

    [Fact]
    public void RequiredField_WhenFieldExists_ShouldPass()
    {
        // Arrange
        var rule = new ValidationRule
        {
            Type = "Required",
            Configuration = "credentialSubject.id"
        };
        
        var json = @"{
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            }
        }";
        
        var credential = JsonDocument.Parse(json);
        
        // Act
        var result = ValidationUtility.ValidateRequiredField(credential, rule);
        
        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void RequiredField_WhenFieldDoesNotExist_ShouldFail()
    {
        // Arrange
        var rule = new ValidationRule
        {
            Type = "Required",
            Configuration = "credentialSubject.missing"
        };
        
        var json = @"{
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            }
        }";
        
        var credential = JsonDocument.Parse(json);
        
        // Act
        var result = ValidationUtility.ValidateRequiredField(credential, rule);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("is missing");
    }

    [Fact]
    public void RequiredField_WithNestedPath_ShouldFindField()
    {
        // Arrange
        var rule = new ValidationRule
        {
            Type = "Required",
            Configuration = "credentialSubject.address.city"
        };
        
        var json = @"{
            ""credentialSubject"": {
                ""id"": ""did:example:123"",
                ""address"": {
                    ""city"": ""New York"",
                    ""country"": ""USA""
                }
            }
        }";
        
        var credential = JsonDocument.Parse(json);
        
        // Act
        var result = ValidationUtility.ValidateRequiredField(credential, rule);
        
        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void RequiredField_WithEmptyValue_ShouldPass()
    {
        // Arrange
        var rule = new ValidationRule
        {
            Type = "Required",
            Configuration = "credentialSubject.name"
        };
        
        var json = @"{
            ""credentialSubject"": {
                ""id"": ""did:example:123"",
                ""name"": """"
            }
        }";
        
        var credential = JsonDocument.Parse(json);
        
        // Act
        var result = ValidationUtility.ValidateRequiredField(credential, rule);
        
        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void RequiredField_WithNullValue_ShouldPass()
    {
        // Arrange
        var rule = new ValidationRule
        {
            Type = "Required",
            Configuration = "credentialSubject.name"
        };
        
        var json = @"{
            ""credentialSubject"": {
                ""id"": ""did:example:123"",
                ""name"": null
            }
        }";
        
        var credential = JsonDocument.Parse(json);
        
        // Act
        var result = ValidationUtility.ValidateRequiredField(credential, rule);
        
        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void RequiredField_WithArrayIndex_ShouldFindElement()
    {
        // Arrange
        var rule = new ValidationRule
        {
            Type = "Required",
            Configuration = "credentialSubject.achievements[0].type"
        };
        
        var json = @"{
            ""credentialSubject"": {
                ""id"": ""did:example:123"",
                ""achievements"": [
                    { ""type"": ""Award"", ""name"": ""Best Performance"" },
                    { ""type"": ""Certificate"", ""name"": ""Completion"" }
                ]
            }
        }";
        
        var credential = JsonDocument.Parse(json);
        
        // Act
        var result = ValidationUtility.ValidateRequiredField(credential, rule);
        
        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    #endregion
    
    #region Format Rules Tests

    [Fact]
    public void FormatRule_ValidISO8601Date_ShouldPass()
    {
        // Arrange
        var rule = new ValidationRule
        {
            Type = "Format",
            Configuration = "issuanceDate:ISO8601"
        };
        
        var json = @"{
            ""issuanceDate"": ""2023-01-01T12:00:00Z""
        }";
        
        var credential = JsonDocument.Parse(json);
        
        // Act
        var result = ValidationUtility.ValidateFormat(credential, rule);
        
        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void FormatRule_InvalidISO8601Date_ShouldFail()
    {
        // Arrange
        var rule = new ValidationRule
        {
            Type = "Format",
            Configuration = "issuanceDate:ISO8601"
        };
        
        var json = @"{
            ""issuanceDate"": ""2023-13-01""
        }";
        
        var credential = JsonDocument.Parse(json);
        
        // Act
        var result = ValidationUtility.ValidateFormat(credential, rule);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not a valid ISO8601 date");
    }

    [Fact]
    public void FormatRule_ValidEmail_ShouldPass()
    {
        // Arrange
        var rule = new ValidationRule
        {
            Type = "Format",
            Configuration = "credentialSubject.email:EMAIL"
        };
        
        var json = @"{
            ""credentialSubject"": {
                ""email"": ""user@example.com""
            }
        }";
        
        var credential = JsonDocument.Parse(json);
        
        // Act
        var result = ValidationUtility.ValidateFormat(credential, rule);
        
        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void FormatRule_InvalidEmail_ShouldFail()
    {
        // Arrange
        var rule = new ValidationRule
        {
            Type = "Format",
            Configuration = "credentialSubject.email:EMAIL"
        };
        
        var json = @"{
            ""credentialSubject"": {
                ""email"": ""not-an-email""
            }
        }";
        
        var credential = JsonDocument.Parse(json);
        
        // Act
        var result = ValidationUtility.ValidateFormat(credential, rule);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not a valid email format");
    }

    [Fact]
    public void FormatRule_ValidUrl_ShouldPass()
    {
        // Arrange
        var rule = new ValidationRule
        {
            Type = "Format",
            Configuration = "credentialSubject.website:URL"
        };
        
        var json = @"{
            ""credentialSubject"": {
                ""website"": ""https://example.com""
            }
        }";
        
        var credential = JsonDocument.Parse(json);
        
        // Act
        var result = ValidationUtility.ValidateFormat(credential, rule);
        
        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void FormatRule_InvalidUrl_ShouldFail()
    {
        // Arrange
        var rule = new ValidationRule
        {
            Type = "Format",
            Configuration = "credentialSubject.website:URL"
        };
        
        var json = @"{
            ""credentialSubject"": {
                ""website"": ""not-a-url""
            }
        }";
        
        var credential = JsonDocument.Parse(json);
        
        // Act
        var result = ValidationUtility.ValidateFormat(credential, rule);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not a valid URL format");
    }

    [Fact]
    public void FormatRule_ValidDid_ShouldPass()
    {
        // Arrange
        var rule = new ValidationRule
        {
            Type = "Format",
            Configuration = "credentialSubject.id:DID"
        };
        
        var json = @"{
            ""credentialSubject"": {
                ""id"": ""did:example:123456""
            }
        }";
        
        var credential = JsonDocument.Parse(json);
        
        // Act
        var result = ValidationUtility.ValidateFormat(credential, rule);
        
        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void FormatRule_InvalidDid_ShouldFail()
    {
        // Arrange
        var rule = new ValidationRule
        {
            Type = "Format",
            Configuration = "credentialSubject.id:DID"
        };
        
        var json = @"{
            ""credentialSubject"": {
                ""id"": ""example:123456""
            }
        }";
        
        var credential = JsonDocument.Parse(json);
        
        // Act
        var result = ValidationUtility.ValidateFormat(credential, rule);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not a valid DID format");
    }

    [Fact]
    public void FormatRule_UnsupportedFormat_ShouldFail()
    {
        // Arrange
        var rule = new ValidationRule
        {
            Type = "Format",
            Configuration = "credentialSubject.phone:PHONE"
        };
        
        var json = @"{
            ""credentialSubject"": {
                ""phone"": ""+1-555-555-5555""
            }
        }";
        
        var credential = JsonDocument.Parse(json);
        
        // Act
        var result = ValidationUtility.ValidateFormat(credential, rule);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Unsupported format");
    }

    [Fact]
    public void FormatRule_InvalidConfiguration_ShouldFail()
    {
        // Arrange
        var rule = new ValidationRule
        {
            Type = "Format",
            Configuration = "credentialSubject.email" // Missing format specifier
        };
        
        var json = @"{
            ""credentialSubject"": {
                ""email"": ""user@example.com""
            }
        }";
        
        var credential = JsonDocument.Parse(json);
        
        // Act
        var result = ValidationUtility.ValidateFormat(credential, rule);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid format rule configuration");
    }

    #endregion
    
    #region Range Rules Tests

    [Fact]
    public void RangeRule_NumberInRange_ShouldPass()
    {
        // Arrange
        var rule = new ValidationRule
        {
            Type = "Range",
            Configuration = "credentialSubject.age:18-65"
        };
        
        var json = @"{
            ""credentialSubject"": {
                ""age"": 30
            }
        }";
        
        var credential = JsonDocument.Parse(json);
        
        // Act
        var result = ValidationUtility.ValidateRange(credential, rule);
        
        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void RangeRule_NumberAtMinBoundary_ShouldPass()
    {
        // Arrange
        var rule = new ValidationRule
        {
            Type = "Range",
            Configuration = "credentialSubject.age:18-65"
        };
        
        var json = @"{
            ""credentialSubject"": {
                ""age"": 18
            }
        }";
        
        var credential = JsonDocument.Parse(json);
        
        // Act
        var result = ValidationUtility.ValidateRange(credential, rule);
        
        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void RangeRule_NumberAtMaxBoundary_ShouldPass()
    {
        // Arrange
        var rule = new ValidationRule
        {
            Type = "Range",
            Configuration = "credentialSubject.age:18-65"
        };
        
        var json = @"{
            ""credentialSubject"": {
                ""age"": 65
            }
        }";
        
        var credential = JsonDocument.Parse(json);
        
        // Act
        var result = ValidationUtility.ValidateRange(credential, rule);
        
        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void RangeRule_NumberBelowRange_ShouldFail()
    {
        // Arrange
        var rule = new ValidationRule
        {
            Type = "Range",
            Configuration = "credentialSubject.age:18-65"
        };
        
        var json = @"{
            ""credentialSubject"": {
                ""age"": 17
            }
        }";
        
        var credential = JsonDocument.Parse(json);
        
        // Act
        var result = ValidationUtility.ValidateRange(credential, rule);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("outside range");
    }

    [Fact]
    public void RangeRule_NumberAboveRange_ShouldFail()
    {
        // Arrange
        var rule = new ValidationRule
        {
            Type = "Range",
            Configuration = "credentialSubject.age:18-65"
        };
        
        var json = @"{
            ""credentialSubject"": {
                ""age"": 66
            }
        }";
        
        var credential = JsonDocument.Parse(json);
        
        // Act
        var result = ValidationUtility.ValidateRange(credential, rule);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("outside range");
    }

    [Fact]
    public void RangeRule_NumberAsString_ShouldBeValidated()
    {
        // Arrange
        var rule = new ValidationRule
        {
            Type = "Range",
            Configuration = "credentialSubject.age:18-65"
        };
        
        var json = @"{
            ""credentialSubject"": {
                ""age"": ""30""
            }
        }";
        
        var credential = JsonDocument.Parse(json);
        
        // Act
        var result = ValidationUtility.ValidateRange(credential, rule);
        
        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void RangeRule_NonNumericString_ShouldFail()
    {
        // Arrange
        var rule = new ValidationRule
        {
            Type = "Range",
            Configuration = "credentialSubject.age:18-65"
        };
        
        var json = @"{
            ""credentialSubject"": {
                ""age"": ""thirty""
            }
        }";
        
        var credential = JsonDocument.Parse(json);
        
        // Act
        var result = ValidationUtility.ValidateRange(credential, rule);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("cannot be parsed as a number");
    }

    [Fact]
    public void RangeRule_WithNegativeRange_ShouldWork()
    {
        // Arrange
        var rule = new ValidationRule
        {
            Type = "Range",
            Configuration = "credentialSubject.temperature:-20-40"
        };
        
        var json = @"{
            ""credentialSubject"": {
                ""temperature"": -15
            }
        }";
        
        var credential = JsonDocument.Parse(json);
        
        // Act
        var result = ValidationUtility.ValidateRange(credential, rule);
        
        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void RangeRule_WithDecimalValues_ShouldWork()
    {
        // Arrange
        var rule = new ValidationRule
        {
            Type = "Range",
            Configuration = "credentialSubject.score:0.5-9.5"
        };
        
        var json = @"{
            ""credentialSubject"": {
                ""score"": 7.5
            }
        }";
        
        var credential = JsonDocument.Parse(json);
        
        // Act
        var result = ValidationUtility.ValidateRange(credential, rule);
        
        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void RangeRule_InvalidConfiguration_ShouldFail()
    {
        // Arrange
        var rule = new ValidationRule
        {
            Type = "Range",
            Configuration = "credentialSubject.age:invalid" // Not a valid range format
        };
        
        var json = @"{
            ""credentialSubject"": {
                ""age"": 30
            }
        }";
        
        var credential = JsonDocument.Parse(json);
        
        // Act
        var result = ValidationUtility.ValidateRange(credential, rule);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid range format");
    }

    [Fact]
    public void RangeRule_FieldNotFound_ShouldFail()
    {
        // Arrange
        var rule = new ValidationRule
        {
            Type = "Range",
            Configuration = "credentialSubject.missingField:18-65"
        };
        
        var json = @"{
            ""credentialSubject"": {
                ""age"": 30
            }
        }";
        
        var credential = JsonDocument.Parse(json);
        
        // Act
        var result = ValidationUtility.ValidateRange(credential, rule);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    #endregion
    
    #region Custom Rules Tests

    [Fact]
    public void CustomRule_NotImplemented_ShouldFail()
    {
        // Arrange
        var rule = new ValidationRule
        {
            Type = "Custom",
            Configuration = "data.value > 10"
        };
        
        var json = @"{
            ""value"": 20
        }";
        
        var credential = JsonDocument.Parse(json);
        
        // Act
        var result = ValidationUtility.ValidateCustomRule(credential, rule);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not implemented");
    }

    #endregion

    #region Custom JavaScript Rules Tests
    
    [Fact]
    public void JavaScriptRule_SimpleCondition_Valid_ShouldPass()
    {
        // Arrange
        var rule = new CustomValidationRule
        {
            Name = "NameRule",
            Expression = "data.person.name && data.person.name.length > 2",
            ErrorMessage = "Name is required and must be at least 3 characters"
        };
        
        var json = @"{
            ""person"": {
                ""name"": ""John Doe"",
                ""age"": 30
            }
        }";
        
        var data = JsonSerializer.Deserialize<JsonElement>(json);
        
        // Act
        var result = ValidationUtility.ValidateJavaScriptExpression(data, rule);
        
        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }
    
    [Fact]
    public void JavaScriptRule_SimpleCondition_Invalid_ShouldFail()
    {
        // Arrange
        var rule = new CustomValidationRule
        {
            Name = "NameRule",
            Expression = "data.person.name && data.person.name.length > 2",
            ErrorMessage = "Name is required and must be at least 3 characters"
        };
        
        var json = @"{
            ""person"": {
                ""name"": ""Jo"",
                ""age"": 30
            }
        }";
        
        var data = JsonSerializer.Deserialize<JsonElement>(json);
        
        // Act
        var result = ValidationUtility.ValidateJavaScriptExpression(data, rule);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Name is required and must be at least 3 characters");
    }
    
    [Fact]
    public void JavaScriptRule_NumberComparison_Valid_ShouldPass()
    {
        // Arrange
        var rule = new CustomValidationRule
        {
            Name = "AgeRule",
            Expression = "data.person.age >= 18",
            ErrorMessage = "Age must be 18 or above"
        };
        
        var json = @"{
            ""person"": {
                ""name"": ""John Doe"",
                ""age"": 30
            }
        }";
        
        var data = JsonSerializer.Deserialize<JsonElement>(json);
        
        // Act
        var result = ValidationUtility.ValidateJavaScriptExpression(data, rule);
        
        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }
    
    [Fact]
    public void JavaScriptRule_NumberComparison_Invalid_ShouldFail()
    {
        // Arrange
        var rule = new CustomValidationRule
        {
            Name = "AgeRule",
            Expression = "data.person.age >= 18",
            ErrorMessage = "Age must be 18 or above"
        };
        
        var json = @"{
            ""person"": {
                ""name"": ""John Doe"",
                ""age"": 16
            }
        }";
        
        var data = JsonSerializer.Deserialize<JsonElement>(json);
        
        // Act
        var result = ValidationUtility.ValidateJavaScriptExpression(data, rule);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Age must be 18 or above");
    }
    
    [Fact]
    public void JavaScriptRule_MalformedExpression_ShouldReturnError()
    {
        // Arrange
        var rule = new CustomValidationRule
        {
            Name = "InvalidSyntaxRule",
            Expression = "data.person.name === 'John' &&& data.person.age > 20", // invalid syntax
            ErrorMessage = "This rule has syntax errors"
        };
        
        var json = @"{
            ""person"": {
                ""name"": ""John Doe"",
                ""age"": 30
            }
        }";
        
        var data = JsonSerializer.Deserialize<JsonElement>(json);
        
        // Act
        var result = ValidationUtility.ValidateJavaScriptExpression(data, rule);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Error evaluating rule");
    }
    
    [Fact]
    public void JavaScriptRule_ComplexObjectAccess_ShouldWork()
    {
        // Arrange
        var rule = new CustomValidationRule
        {
            Name = "DepartmentCountRule",
            Expression = "data.organization.departments.length >= 2",
            ErrorMessage = "Organization must have at least 2 departments"
        };
        
        var json = @"{
            ""organization"": {
                ""name"": ""Acme Corp"",
                ""departments"": [
                    { ""name"": ""Engineering"", ""employeeCount"": 50 },
                    { ""name"": ""Marketing"", ""employeeCount"": 20 }
                ]
            }
        }";
        
        var data = JsonSerializer.Deserialize<JsonElement>(json);
        
        // Act
        var result = ValidationUtility.ValidateJavaScriptExpression(data, rule);
        
        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }
    
    [Fact]
    public void JavaScriptRule_ArrayOperations_ShouldWork()
    {
        // Arrange
        var rule = new CustomValidationRule
        {
            Name = "PriceCheckRule",
            Expression = "data.items.some(item => item.price < 6.00)",
            ErrorMessage = "Must have at least one item under $6.00"
        };
        
        var json = @"{
            ""items"": [
                { ""id"": 1, ""name"": ""Item 1"", ""price"": 10.99 },
                { ""id"": 2, ""name"": ""Item 2"", ""price"": 20.50 },
                { ""id"": 3, ""name"": ""Item 3"", ""price"": 5.75 }
            ]
        }";
        
        var data = JsonSerializer.Deserialize<JsonElement>(json);
        
        // Act
        var result = ValidationUtility.ValidateJavaScriptExpression(data, rule);
        
        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }
    
    [Fact]
    public void JavaScriptRule_DateComparison_ShouldWork()
    {
        // Arrange
        var rule = new CustomValidationRule
        {
            Name = "RegistrationDateRule",
            Expression = "new Date(data.registrationDate) <= new Date()",
            ErrorMessage = "Registration date cannot be in the future"
        };
        
        var json = @"{
            ""registrationDate"": ""2023-01-01T12:00:00Z""
        }";
        
        var data = JsonSerializer.Deserialize<JsonElement>(json);
        
        // Act
        var result = ValidationUtility.ValidateJavaScriptExpression(data, rule);
        
        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }
    
    #endregion
    
   
}