using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Outgoing;
using Blocktrust.CredentialWorkflow.Core.Services;
using Blocktrust.CredentialWorkflow.Core.Settings;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Blocktrust.CredentialWorkflow.Core.Tests.Services.EmailServiceTests
{
    public class EmailServiceTests
    {
        private readonly Mock<IOptions<EmailSettings>> _emailSettingsMock;
        private readonly Mock<ILogger<EmailService>> _loggerMock;
        private readonly EmailSettings _emailSettings;
        private readonly EmailService _emailService;

        public EmailServiceTests()
        {
            _emailSettings = new EmailSettings
            {
                SendGridKey = "test-key",
                SendGridFromEmail = "test@example.com",
                DefaultFromName = "Test Sender"
            };

            _emailSettingsMock = new Mock<IOptions<EmailSettings>>();
            _emailSettingsMock.Setup(x => x.Value).Returns(_emailSettings);
            
            _loggerMock = new Mock<ILogger<EmailService>>();
            
            _emailService = new EmailService(_emailSettingsMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void ProcessTemplate_ReplacesParameters()
        {
            // Arrange
            var template = "Hello {{name}}, welcome to {{service}}!";
            var parameters = new Dictionary<string, string>
            {
                { "name", "John" },
                { "service", "CredentialWorkflow" }
            };

            // Act
            var result = _emailService.ProcessTemplate(template, parameters);

            // Assert
            result.Should().Be("Hello John, welcome to CredentialWorkflow!");
        }

        [Fact]
        public void ProcessTemplate_KeepsUnknownParameters()
        {
            // Arrange
            var template = "Hello {{name}}, welcome to {{service}}!";
            var parameters = new Dictionary<string, string>
            {
                { "name", "John" }
                // Missing service parameter
            };

            // Act
            var result = _emailService.ProcessTemplate(template, parameters);

            // Assert
            result.Should().Be("Hello John, welcome to {{service}}!");
        }

        [Fact]
        public void ProcessTemplate_HandlesEmptyParameters()
        {
            // Arrange
            var template = "Hello {{name}}, welcome to {{service}}!";
            var parameters = new Dictionary<string, string>();

            // Act
            var result = _emailService.ProcessTemplate(template, parameters);

            // Assert
            result.Should().Be("Hello {{name}}, welcome to {{service}}!");
        }
    }
}