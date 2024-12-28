using Blocktrust.CredentialWorkflow.Core.Commands.VerifyCredentials.VerifyW3cCredentials.CheckExpiry;
using Blocktrust.CredentialWorkflow.Core.Domain.Credential;
using FluentAssertions;

namespace Blocktrust.CredentialWorkflow.Core.Tests.Commands.VerifyCredentialsTests.VerifyW3cCredentialTests;

public class CheckExpiryTests
{
    private readonly CheckExpiryHandler _handler;

    public CheckExpiryTests()
    {
        _handler = new CheckExpiryHandler();
    }

    [Fact]
    public async Task Handle_CredentialWithValidUntil_NotExpired_ShouldReturnFalse()
    {
        // Arrange
        var credential = new Credential
        {
            ValidUntil = DateTime.UtcNow.AddDays(1),
            CredentialContext = null,
            Type = null,
            CredentialSubjects = null
        };
        var request = new CheckExpiryRequest(credential);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_CredentialWithValidUntil_Expired_ShouldReturnTrue()
    {
        // Arrange
        var credential = new Credential
        {
            ValidUntil = DateTime.UtcNow.AddDays(-1),
            CredentialContext = null,
            Type = null,
            CredentialSubjects = null
        };
        var request = new CheckExpiryRequest(credential);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_CredentialWithExpirationDate_NotExpired_ShouldReturnFalse()
    {
        // Arrange
        var credential = new Credential
        {
            ExpirationDate = DateTime.UtcNow.AddDays(1),
            CredentialContext = null,
            Type = null,
            CredentialSubjects = null
        };
        var request = new CheckExpiryRequest(credential);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_CredentialWithExpirationDate_Expired_ShouldReturnTrue()
    {
        // Arrange
        var credential = new Credential
        {
            ExpirationDate = DateTime.UtcNow.AddDays(-1),
            CredentialContext = null,
            Type = null,
            CredentialSubjects = null
        };
        var request = new CheckExpiryRequest(credential);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_CredentialWithNoExpiryDates_ShouldReturnFalse()
    {
        // Arrange
        var credential = new Credential
        {
            CredentialContext = null,
            Type = null,
            CredentialSubjects = null
        };
        var request = new CheckExpiryRequest(credential);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_CredentialWithBothDates_ShouldPreferValidUntil()
    {
        // Arrange
        var credential = new Credential
        {
            ValidUntil = DateTime.UtcNow.AddDays(1), // Not expired
            ExpirationDate = DateTime.UtcNow.AddDays(-1),
            CredentialContext = null,
            Type = null,
            CredentialSubjects = null // Expired
        };
        var request = new CheckExpiryRequest(credential);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse(); // Should use ValidUntil which is not expired
    }
}