using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.CreateIssuingKey;
using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.CreateTenant;
using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.GetPrivateIssuingKeyByDid;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Blocktrust.CredentialWorkflow.Core.Tests.Commands.IssuingKeyTests;

public class GetPrivateIssuingKeyByDidHandlerTests : TestSetup
{
    private readonly GetPrivateIssuingKeyByDidHandler _handler;
    private readonly CreateIssuingKeyHandler _createIssuingKeyHandler;
    private readonly CreateTenantHandler _createTenantHandler;
    private readonly DataContext _dataContext;

    public GetPrivateIssuingKeyByDidHandlerTests(TransactionalTestDatabaseFixture fixture) : base(fixture)
    {
        _dataContext = fixture.CreateContext();
        _handler = new GetPrivateIssuingKeyByDidHandler(_dataContext);
        _createIssuingKeyHandler = new CreateIssuingKeyHandler(_dataContext);
        _createTenantHandler = new CreateTenantHandler(_dataContext);
    }

    [Fact]
    public async Task Handle_NonExistentDid_ShouldReturnFailure()
    {
        // Arrange
        var nonExistentDid = "did:prism:nonexistent123";
        var request = new GetPrivateIssuingKeyByDidRequest(nonExistentDid);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be("Issuing key not found for the provided DID.");

        var keyInDb = await _dataContext.IssuingKeys
            .FirstOrDefaultAsync(k => k.Did == nonExistentDid);
        keyInDb.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("not-a-did")]
    [InlineData("did:")]
    [InlineData("did:invalidformat")]
    [InlineData("prism:123")]
    public async Task Handle_InvalidDidFormat_ShouldReturnFailure(string invalidDid)
    {
        // Arrange
        var request = new GetPrivateIssuingKeyByDidRequest(invalidDid);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be("Issuing key not found for the provided DID.");
    }

    [Fact]
    public async Task Handle_MultipleDidKeys_ShouldReturnCorrectPrivateKey()
    {
        // Arrange
        var tenantResult = await _createTenantHandler.Handle(
            new CreateTenantRequest("TestTenant"),
            CancellationToken.None);
        tenantResult.IsSuccess.Should().BeTrue();

        // Create multiple keys with different DIDs
        var testData = new[]
        {
            ("TestKey1", "did:prism:test1", "privateKey1"),
            ("TestKey2", "did:prism:test2", "privateKey2"),
            ("TestKey3", "did:prism:test3", "privateKey3")
        };

        foreach (var (name, did, privateKey) in testData)
        {
            var createResult = await _createIssuingKeyHandler.Handle(
                new CreateIssuingKeyRequest(
                    tenantResult.Value,
                    name,
                    did,
                    "secp256k1",
                    privateKey,
                    "publicKey",
                    null),
                CancellationToken.None);
            createResult.IsSuccess.Should().BeTrue();
        }

        // Verify each private key can be retrieved correctly
        foreach (var (_, did, expectedPrivateKey) in testData)
        {
            // Act
            var result = await _handler.Handle(
                new GetPrivateIssuingKeyByDidRequest(did),
                CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(expectedPrivateKey);
        }

        // Verify database state
        var keysInDb = await _dataContext.IssuingKeys
            .Where(k => k.TenantEntityId == tenantResult.Value)
            .ToListAsync();
        keysInDb.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_DuplicateDidKeys_ShouldReturnFirstMatch()
    {
        // Arrange
        var tenantResult = await _createTenantHandler.Handle(
            new CreateTenantRequest("TestTenant"),
            CancellationToken.None);
        tenantResult.IsSuccess.Should().BeTrue();

        // Create multiple keys with same DID but different private keys
        var sameDid = "did:prism:duplicate";
        var keys = new[]
        {
            ("Key1", "privateKey1"),
            ("Key2", "privateKey2")
        };

        foreach (var (name, privateKey) in keys)
        {
            var createResult = await _createIssuingKeyHandler.Handle(
                new CreateIssuingKeyRequest(
                    tenantResult.Value,
                    name,
                    sameDid,
                    "secp256k1",
                    privateKey,
                    "publicKey",
                    null),
                CancellationToken.None);
            createResult.IsSuccess.Should().BeTrue();
        }

        // Act
        var result = await _handler.Handle(
            new GetPrivateIssuingKeyByDidRequest(sameDid),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("privateKey1"); // Should return first matching key

        // Verify both keys exist in database
        var keysInDb = await _dataContext.IssuingKeys
            .Where(k => k.Did == sameDid)
            .ToListAsync();
        keysInDb.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_CaseSensitiveDid_ShouldNotMatchDifferentCase()
    {
        // Arrange
        var tenantResult = await _createTenantHandler.Handle(
            new CreateTenantRequest("TestTenant"),
            CancellationToken.None);
        tenantResult.IsSuccess.Should().BeTrue();

        var did = "did:prism:TEST123";
        var createResult = await _createIssuingKeyHandler.Handle(
            new CreateIssuingKeyRequest(
                tenantResult.Value,
                "TestKey",
                did,
                "secp256k1",
                "privateKey",
                "publicKey",
                null),
            CancellationToken.None);
        createResult.IsSuccess.Should().BeTrue();

        // Act
        var result = await _handler.Handle(
            new GetPrivateIssuingKeyByDidRequest(did.ToLower()),
            CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be("Issuing key not found for the provided DID.");
    }
}