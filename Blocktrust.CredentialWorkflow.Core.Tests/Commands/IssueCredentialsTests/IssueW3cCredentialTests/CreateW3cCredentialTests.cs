using FluentAssertions;
using Blocktrust.CredentialWorkflow.Core.Commands.IssueCredentials.IssueW3cCredential.CreateW3cCredential;

namespace Blocktrust.CredentialWorkflow.Core.Tests.Commands.IssueCredentialsTests.IssueW3cCredentialTests;

public class CreateW3cCredentialTests
{
    private readonly CreateW3cCredentialHandler _handler;
    
    private const string IssuerDid = "did:prism:issuer123";
    private const string SubjectDid = "did:prism:subject456";

    public CreateW3cCredentialTests()
    {
        _handler = new CreateW3cCredentialHandler();
    }

    [Fact]
    public async Task Handle_MinimalCredential_ShouldCreateValidCredential()
    {
        // Arrange
        var request = new CreateW3cCredentialRequest(IssuerDid, SubjectDid);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var credential = result.Value;
        
        // Verify Context
        credential.CredentialContext.Contexts.Should().ContainSingle()
            .Which.Should().Be("https://www.w3.org/2018/credentials/v1");
        
        // Verify Type
        credential.Type.Type.Should().BeEquivalentTo(new HashSet<string> { "VerifiableCredential" });
        
        // Verify Issuer
        credential.CredentialIssuer.IssuerId.ToString().Should().Be(IssuerDid);
        
        // Verify Subject
        credential.CredentialSubjects.Should().ContainSingle();
        var subject = credential.CredentialSubjects.Single();
        subject.Id.ToString().Should().Be(SubjectDid);
        subject.AdditionalData.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task Handle_WithAdditionalData_ShouldIncludeInSubject()
    {
        // Arrange
        var additionalData = new Dictionary<string, object>
        {
            { "name", "John Doe" },
            { "age", 30 }
        };
        
        var request = new CreateW3cCredentialRequest(
            IssuerDid, 
            SubjectDid,
            additionalData);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var subject = result.Value.CredentialSubjects.Single();
        subject.AdditionalData.Should().BeEquivalentTo(additionalData);
    }

    [Fact]
    public async Task Handle_WithCustomDates_ShouldSetValidityPeriod()
    {
        // Arrange
        var validFrom = DateTimeOffset.UtcNow;
        var expirationDate = validFrom.AddYears(1);
        
        var request = new CreateW3cCredentialRequest(
            IssuerDid, 
            SubjectDid,
            validFrom: validFrom,
            expirationDate: expirationDate);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var credential = result.Value;
        credential.ValidFrom.Should().Be(validFrom.DateTime);
        credential.ExpirationDate.Should().Be(expirationDate.DateTime);
    }

    [Fact]
    public async Task Handle_WithInvalidDid_ShouldReturnFailure()
    {
        // Arrange
        var invalidDid = "not-a-did";
        var request = new CreateW3cCredentialRequest(invalidDid, SubjectDid);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Contain("Failed to create credential");
    }

    [Fact]
    public async Task Handle_WithDefaultValues_ShouldSetCurrentTimeAsValidFrom()
    {
        // Arrange
        var beforeTest = DateTime.UtcNow;
        var request = new CreateW3cCredentialRequest(IssuerDid, SubjectDid);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);
        var afterTest = DateTime.UtcNow;

        // Assert
        result.IsSuccess.Should().BeTrue();
        var credential = result.Value;
        credential.ValidFrom.Should().BeOnOrAfter(beforeTest);
        credential.ValidFrom.Should().BeOnOrBefore(afterTest);
        credential.ExpirationDate.Should().BeNull();
    }
}