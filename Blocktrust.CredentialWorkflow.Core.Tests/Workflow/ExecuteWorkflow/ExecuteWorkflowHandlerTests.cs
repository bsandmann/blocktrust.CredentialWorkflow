using FluentAssertions;
using System.Collections.Generic;
using Xunit;

namespace Blocktrust.CredentialWorkflow.Core.Tests.Workflow.ExecuteWorkflow;

using Core.Commands.Workflow.ExecuteWorkflow.ActionProcessors;

public class EmailTemplateProcessorTests
{
    [Fact]
    public void ProcessEmailTemplate_WithNullTemplate_ShouldReturnEmptyString()
    {
        // Arrange
        string? template = null;
        var parameters = new Dictionary<string, string>
        {
            { "name", "John" }
        };

        // Act
        var result = EmailActionProcessor.ProcessEmailTemplate(template, parameters);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ProcessEmailTemplate_WithEmptyTemplate_ShouldReturnEmptyString()
    {
        // Arrange
        var template = string.Empty;
        var parameters = new Dictionary<string, string>
        {
            { "name", "John" }
        };

        // Act
        var result = EmailActionProcessor.ProcessEmailTemplate(template, parameters);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ProcessEmailTemplate_WithNullParameters_ShouldReturnOriginalTemplate()
    {
        // Arrange
        var template = "Hello [name], welcome!";
        Dictionary<string, string>? parameters = null;

        // Act
        var result = EmailActionProcessor.ProcessEmailTemplate(template, parameters);

        // Assert
        result.Should().Be(template);
    }

    [Fact]
    public void ProcessEmailTemplate_WithEmptyParameters_ShouldReturnOriginalTemplate()
    {
        // Arrange
        var template = "Hello [name], welcome!";
        var parameters = new Dictionary<string, string>();

        // Act
        var result = EmailActionProcessor.ProcessEmailTemplate(template, parameters);

        // Assert
        result.Should().Be(template);
    }

    [Fact]
    public void ProcessEmailTemplate_WithSingleParameter_ShouldReplaceCorrectly()
    {
        // Arrange
        var template = "Hello [name], welcome!";
        var parameters = new Dictionary<string, string>
        {
            { "name", "John" }
        };

        // Act
        var result = EmailActionProcessor.ProcessEmailTemplate(template, parameters);

        // Assert
        result.Should().Be("Hello John, welcome!");
    }

    [Fact]
    public void ProcessEmailTemplate_WithMultipleParameters_ShouldReplaceAll()
    {
        // Arrange
        var template = "Hello [name], welcome to [company]!";
        var parameters = new Dictionary<string, string>
        {
            { "name", "John" },
            { "company", "Acme Corp" }
        };

        // Act
        var result = EmailActionProcessor.ProcessEmailTemplate(template, parameters);

        // Assert
        result.Should().Be("Hello John, welcome to Acme Corp!");
    }

    [Fact]
    public void ProcessEmailTemplate_WithParameterHavingNullValue_ShouldReplaceWithEmptyString()
    {
        // Arrange
        var template = "Hello [name], welcome!";
        var parameters = new Dictionary<string, string>
        {
            { "name", null! }
        };

        // Act
        var result = EmailActionProcessor.ProcessEmailTemplate(template, parameters);

        // Assert
        result.Should().Be("Hello , welcome!");
    }

    [Fact]
    public void ProcessEmailTemplate_WithUnmatchedParameters_ShouldNotReplace()
    {
        // Arrange
        var template = "Hello [name], welcome to [company]!";
        var parameters = new Dictionary<string, string>
        {
            { "name", "John" },
            { "location", "New York" } // This parameter doesn't exist in template
        };

        // Act
        var result = EmailActionProcessor.ProcessEmailTemplate(template, parameters);

        // Assert
        result.Should().Be("Hello John, welcome to [company]!");
    }
}