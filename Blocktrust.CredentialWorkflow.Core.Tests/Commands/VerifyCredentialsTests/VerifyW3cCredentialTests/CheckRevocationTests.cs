using Blocktrust.CredentialBadges.OpenBadges;
using Blocktrust.CredentialWorkflow.Core.Commands.VerifyCredentials.VerifyW3cCredentials.CheckRevocation;
using Blocktrust.CredentialWorkflow.Core.Domain.Credential;
using FluentAssertions;

namespace Blocktrust.CredentialWorkflow.Core.Tests.Commands.VerifyCredentialsTests.VerifyW3cCredentialTests;

public class CheckRevocationTests
{
    private readonly HttpClient _httpClient;
    private readonly CheckRevocationHandler _handler;
    private readonly CredentialParser _parser;

    public CheckRevocationTests()
    {
        _httpClient = new HttpClient();
        _handler = new CheckRevocationHandler(_httpClient);
        _parser = new CredentialParser();
    }

    [Fact]
    public async Task Handle_ValidCredentialNotRevoked_ShouldReturnFalse()
    {
        // Arrange
        var credential = CreateTestCredential(3); // Using index 3 for not revoked
        var request = new CheckRevocationRequest(credential);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ValidCredentialRevoked_ShouldReturnTrue()
    {
        // Arrange
        var credential = CreateTestCredential(1); // Using index 1 for revoked
        var request = new CheckRevocationRequest(credential);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_CredentialWithoutStatus_ShouldReturnFalse()
    {
        // Arrange
        var credential = CreateTestCredentialWithoutStatus();
        var request = new CheckRevocationRequest(credential);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    private Credential CreateTestCredential(int statusListIndex)
    {
        return new Credential
        {
            CredentialStatus = new CredentialStatus
            {
                StatusPurpose = "Revocation",
                StatusListIndex = statusListIndex,
                Id = new Uri(
                    $"http://10.10.50.105:8000/cloud-agent/credential-status/b9b6bb1e-6864-4074-b8ac-12b3a0b30f0c#{statusListIndex}"),
                Type = "StatusList2021Entry",
                StatusListCredential =
                    "http://10.10.50.105:8000/cloud-agent/credential-status/b9b6bb1e-6864-4074-b8ac-12b3a0b30f0c"
            },
            CredentialContext = null,
            Type = null,
            CredentialSubjects = null
        };
    }

    private Credential CreateTestCredentialWithoutStatus()
    {
        return new Credential
        {
            CredentialStatus = null,
            CredentialContext = null,
            Type = null,
            CredentialSubjects = null
        };
    }
}