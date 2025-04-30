using System;
using System.Collections.Generic;
using Blocktrust.CredentialWorkflow.Core.Domain.Template;
using Blocktrust.CredentialWorkflow.Core.Services;
using FluentAssertions;
using Xunit;

namespace Blocktrust.CredentialWorkflow.Core.Tests.Services.TemplateServiceTests
{
    public class TemplateServiceTests
    {
        private readonly TemplateService _templateService;

        public TemplateServiceTests()
        {
            _templateService = new TemplateService();
        }

        [Fact]
        public void ListTemplates_ReturnsAllTemplates()
        {
            // Act
            var result = _templateService.ListTemplates();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Should().BeOfType<List<Template>>();
            result.Value.Count.Should().BeGreaterThan(0); // Should have at least one template
        }

        [Fact]
        public void GetTemplateById_WithValidId_ReturnsTemplate()
        {
            // Arrange
            var templates = _templateService.ListTemplates().Value;
            var templateId = templates[0].Id; // Get the first template's ID

            // Act
            var result = _templateService.GetTemplateById(templateId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(templateId);
        }

        [Fact]
        public void GetTemplateById_WithInvalidId_ReturnsFail()
        {
            // Arrange
            var invalidId = Guid.NewGuid(); // Generate a random ID that doesn't exist

            // Act
            var result = _templateService.GetTemplateById(invalidId);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors[0].Message.Should().Contain("not found");
        }



        [Fact]
        public void GetTemplateById_ReplacesGuidPlaceholders()
        {
            // Arrange
            var templates = _templateService.ListTemplates().Value;
            var templateId = templates[0].Id;

            // Act
            var result = _templateService.GetTemplateById(templateId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            
            // Check that all <GUID> placeholders are replaced
            result.Value.TemplateBody.Should().NotContain("<GUID>");
        }

    }
}