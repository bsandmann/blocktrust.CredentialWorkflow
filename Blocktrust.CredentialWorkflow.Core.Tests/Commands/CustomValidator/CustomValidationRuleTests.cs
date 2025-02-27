using FluentAssertions;
using Jint;

namespace Blocktrust.CredentialWorkflow.Core.Tests.Commands.CustomValidator;

using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Validation;
using System.Text.Json;
using Xunit;

public class CustomValidationRuleTests
{
    // This class tests individual rule evaluations
    // Note: These tests depend on Jint and will fail if Jint is not available
    
    [Fact]
    public void EvaluateRule_SimpleExpression_ShouldEvaluateCorrectly()
    {
        // Arrange
        var rule = new CustomValidationRule
        {
            Name = "SimpleRule",
            Expression = "data.value > 10",
            ErrorMessage = "Value must be greater than 10"
        };
        
        var engine = new Engine();
        var data = new { value = 15 };
        var dataJson = JsonSerializer.Serialize(data);
        engine.SetValue("data", JsonSerializer.Deserialize<JsonElement>(dataJson));
        
        // Act
        var result = engine.Evaluate(rule.Expression).AsBoolean();
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public void EvaluateRule_FailingExpression_ShouldEvaluateCorrectly()
    {
        // Arrange
        var rule = new CustomValidationRule
        {
            Name = "SimpleRule",
            Expression = "data.value > 10",
            ErrorMessage = "Value must be greater than 10"
        };
        
        var engine = new Engine();
        var data = new { value = 5 };
        var dataJson = JsonSerializer.Serialize(data);
        engine.SetValue("data", JsonSerializer.Deserialize<JsonElement>(dataJson));
        
        // Act
        var result = engine.Evaluate(rule.Expression).AsBoolean();
        
        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public void EvaluateRule_StringComparison_ShouldEvaluateCorrectly()
    {
        // Arrange
        var rule = new CustomValidationRule
        {
            Name = "StringRule",
            Expression = "data.name.startsWith('John')",
            ErrorMessage = "Name must start with 'John'"
        };
        
        var engine = new Engine();
        var data = new { name = "John Doe" };
        var dataJson = JsonSerializer.Serialize(data);
        engine.SetValue("data", JsonSerializer.Deserialize<JsonElement>(dataJson));
        
        // Act
        var result = engine.Evaluate(rule.Expression).AsBoolean();
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public void EvaluateRule_ArrayOperation_ShouldEvaluateCorrectly()
    {
        // Arrange
        var rule = new CustomValidationRule
        {
            Name = "ArrayRule",
            Expression = "data.items.length > 2 && data.items.some(item => item.price < 10)",
            ErrorMessage = "Must have more than 2 items and at least one item under $10"
        };
        
        var engine = new Engine();
        var data = new 
        { 
            items = new[]
            {
                new { id = 1, name = "Item 1", price = 15.99 },
                new { id = 2, name = "Item 2", price = 8.50 },
                new { id = 3, name = "Item 3", price = 12.75 }
            }
        };
        var dataJson = JsonSerializer.Serialize(data);
        engine.SetValue("data", JsonSerializer.Deserialize<JsonElement>(dataJson));
        
        // Act
        var result = engine.Evaluate(rule.Expression).AsBoolean();
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public void EvaluateRule_NestedObject_ShouldEvaluateCorrectly()
    {
        // Arrange
        var rule = new CustomValidationRule
        {
            Name = "NestedRule",
            Expression = "data.person.address.city === 'New York' && data.person.contact.email.includes('@')",
            ErrorMessage = "Person must be from New York and have a valid email"
        };
        
        var engine = new Engine();
        var data = new 
        { 
            person = new
            {
                name = "John Doe",
                contact = new
                {
                    email = "john@example.com",
                    phone = "555-1234"
                },
                address = new
                {
                    city = "New York",
                    zipCode = "10001"
                }
            }
        };
        var dataJson = JsonSerializer.Serialize(data);
        engine.SetValue("data", JsonSerializer.Deserialize<JsonElement>(dataJson));
        
        // Act
        var result = engine.Evaluate(rule.Expression).AsBoolean();
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public void EvaluateRule_ComplexCondition_ShouldEvaluateCorrectly()
    {
        // Arrange
        var rule = new CustomValidationRule
        {
            Name = "ComplexRule",
            Expression = @"
                (data.age >= 18 && data.age < 65) &&
                (data.income > 30000 || data.hasInsurance) &&
                (data.education === 'college' || data.yearsExperience > 5)
            ",
            ErrorMessage = "Does not meet complex eligibility criteria"
        };
        
        var engine = new Engine();
        var data = new 
        { 
            age = 35,
            income = 25000,
            hasInsurance = true,
            education = "high school",
            yearsExperience = 10
        };
        var dataJson = JsonSerializer.Serialize(data);
        engine.SetValue("data", JsonSerializer.Deserialize<JsonElement>(dataJson));
        
        // Act
        var result = engine.Evaluate(rule.Expression).AsBoolean();
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public void EvaluateRule_DateComparison_ShouldEvaluateCorrectly()
    {
        // Arrange
        var rule = new CustomValidationRule
        {
            Name = "DateRule",
            Expression = "new Date(data.startDate) < new Date(data.endDate)",
            ErrorMessage = "Start date must be before end date"
        };
        
        var engine = new Engine();
        var data = new 
        { 
            startDate = "2023-01-01T00:00:00Z",
            endDate = "2023-12-31T00:00:00Z"
        };
        var dataJson = JsonSerializer.Serialize(data);
        engine.SetValue("data", JsonSerializer.Deserialize<JsonElement>(dataJson));
        
        // Act
        var result = engine.Evaluate(rule.Expression).AsBoolean();
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public void EvaluateRule_MathExpression_ShouldEvaluateCorrectly()
    {
        // Arrange
        var rule = new CustomValidationRule
        {
            Name = "MathRule",
            Expression = "Math.pow(data.value, 2) > 100 && Math.sqrt(data.value) < 10",
            ErrorMessage = "Value squared must be greater than 100 and square root less than 10"
        };
        
        var engine = new Engine();
        var data = new { value = 25 };
        var dataJson = JsonSerializer.Serialize(data);
        engine.SetValue("data", JsonSerializer.Deserialize<JsonElement>(dataJson));
        
        // Act
        var result = engine.Evaluate(rule.Expression).AsBoolean();
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public void EvaluateRule_ConditionalExpression_ShouldEvaluateCorrectly()
    {
        // Arrange
        var rule = new CustomValidationRule
        {
            Name = "ConditionalRule",
            Expression = "data.type === 'premium' ? data.score > 80 : data.score > 60",
            ErrorMessage = "Score does not meet threshold for account type"
        };
        
        var engine = new Engine();
        var data = new { type = "premium", score = 85 };
        var dataJson = JsonSerializer.Serialize(data);
        engine.SetValue("data", JsonSerializer.Deserialize<JsonElement>(dataJson));
        
        // Act
        var result = engine.Evaluate(rule.Expression).AsBoolean();
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public void EvaluateRule_RegexTest_ShouldEvaluateCorrectly()
    {
        // Arrange
        var rule = new CustomValidationRule
        {
            Name = "RegexRule",
            Expression = "/^[A-Z][a-z]+$/.test(data.firstName)",
            ErrorMessage = "First name must be capitalized and contain only letters"
        };
        
        var engine = new Engine();
        var data = new { firstName = "John" };
        var dataJson = JsonSerializer.Serialize(data);
        engine.SetValue("data", JsonSerializer.Deserialize<JsonElement>(dataJson));
        
        // Act
        var result = engine.Evaluate(rule.Expression).AsBoolean();
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public void EvaluateRule_NullCheckHandling_ShouldEvaluateCorrectly()
    {
        // Arrange
        var rule = new CustomValidationRule
        {
            Name = "NullCheckRule",
            Expression = "data.optional ? data.optional.value > 10 : true",
            ErrorMessage = "Optional value must be greater than 10 if present"
        };
        
        // First test - optional property is present
        var engine1 = new Engine();
        var data1 = new { optional = new { value = 15 } };
        var dataJson1 = JsonSerializer.Serialize(data1);
        engine1.SetValue("data", JsonSerializer.Deserialize<JsonElement>(dataJson1));
        
        // Second test - optional property is missing
        var engine2 = new Engine();
        var data2 = new { different = "property" };
        var dataJson2 = JsonSerializer.Serialize(data2);
        engine2.SetValue("data", JsonSerializer.Deserialize<JsonElement>(dataJson2));
        
        // Act
        var result1 = engine1.Evaluate(rule.Expression).AsBoolean();
        var result2 = engine2.Evaluate(rule.Expression).AsBoolean();
        
        // Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();
    }
    
    [Fact]
    public void EvaluateRule_SyntaxError_ShouldThrowException()
    {
        // Arrange
        var rule = new CustomValidationRule
        {
            Name = "InvalidRule",
            Expression = "data.value > > 10", // Intentional syntax error
            ErrorMessage = "Invalid expression"
        };
        
        var engine = new Engine();
        var data = new { value = 15 };
        var dataJson = JsonSerializer.Serialize(data);
        engine.SetValue("data", JsonSerializer.Deserialize<JsonElement>(dataJson));
        
        // Act & Assert
        Assert.Throws<Jint.Runtime.JavaScriptException>(() => engine.Evaluate(rule.Expression));
    }
    
    [Fact]
    public void EvaluateRule_UndefinedProperty_ShouldReturnFalse()
    {
        // Arrange
        var rule = new CustomValidationRule
        {
            Name = "UndefinedPropertyRule",
            Expression = "data.nonExistent !== undefined && data.nonExistent > 10",
            ErrorMessage = "Non-existent property must be greater than 10"
        };
        
        var engine = new Engine();
        var data = new { value = 15 };
        var dataJson = JsonSerializer.Serialize(data);
        engine.SetValue("data", JsonSerializer.Deserialize<JsonElement>(dataJson));
        
        // Act
        var result = engine.Evaluate(rule.Expression).AsBoolean();
        
        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public void EvaluateRule_DeeplyNestedExpression_ShouldEvaluateCorrectly()
    {
        // Arrange
        var rule = new CustomValidationRule
        {
            Name = "DeeplyNestedRule",
            Expression = @"
                data.company && 
                data.company.departments && 
                data.company.departments.length > 0 && 
                data.company.departments.some(dept => 
                    dept.teams && 
                    dept.teams.some(team => 
                        team.members && 
                        team.members.some(member => 
                            member.role === 'developer' && member.yearsExperience > 3
                        )
                    )
                )
            ",
            ErrorMessage = "Company must have at least one department with a team that has a developer with more than 3 years experience"
        };
        
        var engine = new Engine();
        var data = new 
        { 
            company = new
            {
                name = "TechCorp",
                departments = new[]
                {
                    new
                    {
                        name = "Engineering",
                        teams = new[]
                        {
                            new
                            {
                                name = "Frontend",
                                members = new[]
                                {
                                    new { name = "Alice", role = "developer", yearsExperience = 4 },
                                    new { name = "Bob", role = "designer", yearsExperience = 2 }
                                }
                            }
                        }
                    }
                }
            }
        };
        var dataJson = JsonSerializer.Serialize(data);
        engine.SetValue("data", JsonSerializer.Deserialize<JsonElement>(dataJson));
        
        // Act
        var result = engine.Evaluate(rule.Expression).AsBoolean();
        
        // Assert
        result.Should().BeTrue();
    }
}