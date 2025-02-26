namespace Blocktrust.CredentialWorkflow.Core.Tests.Commands.W3cValidator;
using Domain.ProcessFlow.Actions.Validation;
using FluentAssertions;
using System.Text.Json;
using System.Text.RegularExpressions;
using Xunit;

public class ValidationRuleTests
{
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

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
        var result = ValidateRequiredField(credential, rule);
        
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
        var result = ValidateRequiredField(credential, rule);
        
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
        var result = ValidateRequiredField(credential, rule);
        
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
        var result = ValidateRequiredField(credential, rule);
        
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
        var result = ValidateRequiredField(credential, rule);
        
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
        var result = ValidateRequiredField(credential, rule);
        
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
        var result = ValidateFormat(credential, rule);
        
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
        var result = ValidateFormat(credential, rule);
        
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
        var result = ValidateFormat(credential, rule);
        
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
        var result = ValidateFormat(credential, rule);
        
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
        var result = ValidateFormat(credential, rule);
        
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
        var result = ValidateFormat(credential, rule);
        
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
        var result = ValidateFormat(credential, rule);
        
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
        var result = ValidateFormat(credential, rule);
        
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
        var result = ValidateFormat(credential, rule);
        
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
        var result = ValidateFormat(credential, rule);
        
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
        var result = ValidateRange(credential, rule);
        
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
        var result = ValidateRange(credential, rule);
        
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
        var result = ValidateRange(credential, rule);
        
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
        var result = ValidateRange(credential, rule);
        
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
        var result = ValidateRange(credential, rule);
        
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
        var result = ValidateRange(credential, rule);
        
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
        var result = ValidateRange(credential, rule);
        
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
        var result = ValidateRange(credential, rule);
        
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
        var result = ValidateRange(credential, rule);
        
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
        var result = ValidateRange(credential, rule);
        
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
        var result = ValidateRange(credential, rule);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Field 'credentialSubject.missingField' not found");
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
        var result = ValidateCustomRule(credential, rule);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not implemented");
    }

    #endregion
    
    #region Rule Validation Implementation
    
    private (bool IsValid, string? ErrorMessage) ValidateRequiredField(JsonDocument credential, ValidationRule rule)
    {
        var path = rule.Configuration;
        var pathParts = path.Split('.');
        var element = credential.RootElement;
        
        if (element.ValueKind == JsonValueKind.Null)
        {
            return (false, $"Required field '{path}' is null");
        }
        
        if (!pathParts[0].Equals("vc", StringComparison.OrdinalIgnoreCase) && 
            element.TryGetProperty("vc", out var vcElement))
        {
            element = vcElement;
        }
        
        foreach (var part in pathParts)
        {
            // Check if this part references an array index
            var arrayIndexMatch = Regex.Match(part, @"^(.*)\[(\d+)\]$");
            if (arrayIndexMatch.Success)
            {
                var arrayName = arrayIndexMatch.Groups[1].Value;
                var index = int.Parse(arrayIndexMatch.Groups[2].Value);
                
                if (!element.TryGetProperty(arrayName, out var arrayElement) || 
                    arrayElement.ValueKind != JsonValueKind.Array ||
                    index >= arrayElement.GetArrayLength())
                {
                    return (false, $"Required field '{path}' is missing");
                }
                
                element = arrayElement[index];
                continue;
            }
            
            if (!element.TryGetProperty(part, out var child))
            {
                return (false, $"Required field '{path}' is missing");
            }
            
            element = child;
        }
        
        return (true, null);
    }
    
    private (bool IsValid, string? ErrorMessage) ValidateFormat(JsonDocument credential, ValidationRule rule)
    {
        var config = rule.Configuration.Split(':');
        if (config.Length != 2)
        {
            return (false, "Invalid format rule configuration");
        }
        
        var path = config[0];
        var format = config[1];
        var pathParts = path.Split('.');
        var element = credential.RootElement;
        
        if (!pathParts[0].Equals("vc", StringComparison.OrdinalIgnoreCase) && 
            element.TryGetProperty("vc", out var vcElement))
        {
            element = vcElement;
        }
        
        foreach (var part in pathParts)
        {
            if (!element.TryGetProperty(part, out var child))
            {
                return (false, $"Field '{path}' not found");
            }
            
            element = child;
        }
        
        switch (format.ToUpper())
        {
            case "ISO8601":
                if (element.ValueKind != JsonValueKind.String || 
                    !DateTime.TryParse(element.GetString(), out _))
                {
                    return (false, $"Field '{path}' is not a valid ISO8601 date");
                }
                break;
                
            case "EMAIL":
                if (element.ValueKind != JsonValueKind.String || 
                    !IsValidEmail(element.GetString()))
                {
                    return (false, $"Field '{path}' is not a valid email format");
                }
                break;
                
            case "URL":
                if (element.ValueKind != JsonValueKind.String || 
                    !Uri.TryCreate(element.GetString(), UriKind.Absolute, out _))
                {
                    return (false, $"Field '{path}' is not a valid URL format");
                }
                break;
                
            case "DID":
                if (element.ValueKind != JsonValueKind.String || 
                    !element.GetString().StartsWith("did:"))
                {
                    return (false, $"Field '{path}' is not a valid DID format");
                }
                break;
                
            default:
                return (false, $"Unsupported format '{format}'");
        }
        
        return (true, null);
    }
    
    private (bool IsValid, string? ErrorMessage) ValidateRange(JsonDocument credential, ValidationRule rule)
    {
        var config = rule.Configuration.Split(':');
        if (config.Length != 2)
        {
            return (false, "Invalid range rule configuration");
        }
        
        var path = config[0];
        var range = config[1].Split('-');
        if (range.Length != 2)
        {
            return (false, "Invalid range format");
        }
        
        if (!double.TryParse(range[0], out var min) || !double.TryParse(range[1], out var max))
        {
            return (false, "Invalid range values");
        }
        
        var pathParts = path.Split('.');
        var element = credential.RootElement;
        
        if (!pathParts[0].Equals("vc", StringComparison.OrdinalIgnoreCase) && 
            element.TryGetProperty("vc", out var vcElement))
        {
            element = vcElement;
        }
        
        foreach (var part in pathParts)
        {
            if (!element.TryGetProperty(part, out var child))
            {
                return (false, $"Field '{path}' not found");
            }
            
            element = child;
        }
        
        double value;
        if (element.ValueKind == JsonValueKind.Number)
        {
            try
            {
                value = element.GetDouble();
            }
            catch (InvalidOperationException)
            {
                return (false, $"Field '{path}' cannot be retrieved as a number");
            }
        }
        else if (element.ValueKind == JsonValueKind.String)
        {
            var stringValue = element.GetString();
            if (!double.TryParse(stringValue, out value))
            {
                return (false, $"Field '{path}' is a string but cannot be parsed as a number");
            }
        }
        else
        {
            return (false, $"Field '{path}' is neither a number nor a string");
        }
        
        if (value < min || value > max)
        {
            return (false, $"Field '{path}' value {value} is outside range {min}-{max}");
        }
        
        return (true, null);
    }
    
    private (bool IsValid, string? ErrorMessage) ValidateCustomRule(JsonDocument credential, ValidationRule rule)
    {
        // Custom validation rules are not implemented yet
        return (false, "Custom validation rules are not implemented yet");
    }
    
    private bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
        
        try
        {
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return regex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }
    
    #endregion
}