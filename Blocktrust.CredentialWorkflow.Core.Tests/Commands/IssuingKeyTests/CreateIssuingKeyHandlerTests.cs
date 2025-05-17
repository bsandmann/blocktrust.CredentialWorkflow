using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.CreateIssuingKey;
using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.CreateTenant;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

// using Blocktrust.CredentialWorkflow.Core.Commands.IssuingKey;
// using Blocktrust.CredentialWorkflow.Core.Commands.Tenant;

namespace Blocktrust.CredentialWorkflow.Core.Tests.Commands.IssuingKeyTests;

public class CreateIssuingKeyHandlerTests : TestSetup
{
    private readonly CreateIssuingKeyHandler _handler;
    private readonly CreateTenantHandler _createTenantHandler;
    private readonly DataContext _dataContext;

    public CreateIssuingKeyHandlerTests(TransactionalTestDatabaseFixture fixture) : base(fixture)
    {
        _dataContext = fixture.CreateContext();
        _handler = new CreateIssuingKeyHandler(_dataContext);
        _createTenantHandler = new CreateTenantHandler(_dataContext);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldSucceed()
    {
        // Arrange
        var tenantResult = await _createTenantHandler.Handle(new CreateTenantRequest("TestTenant"), CancellationToken.None);
        tenantResult.IsSuccess.Should().BeTrue();
        var tenantId = tenantResult.Value;

        var request = new CreateIssuingKeyRequest(
            tenantId,
            "TestKey",
            "did:prism:test123",
            "secp256k1",
            "privateKeyTest",
            "publicKeyTest",
            null
        );

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("TestKey");
        result.Value.Did.Should().Be("did:prism:test123");
        result.Value.KeyType.Should().Be("secp256k1");
        result.Value.PublicKey.Should().Be("publicKeyTest");
        result.Value.PrivateKey.Should().Be("privateKeyTest");

        // Verify database state
        var issuingKey = await _dataContext.IssuingKeys
            .FirstOrDefaultAsync(i => i.TenantEntityId == tenantId);
        issuingKey.Should().NotBeNull();
        issuingKey!.Name.Should().Be("TestKey");
        issuingKey.Did.Should().Be("did:prism:test123");
        issuingKey.KeyType.Should().Be("secp256k1");
        issuingKey.PublicKey.Should().Be("publicKeyTest");
        issuingKey.PrivateKey.Should().Be("privateKeyTest");
    }

    [Fact]
    public async Task Handle_NonExistentTenant_ShouldFail()
    {
        // Arrange
        var request = new CreateIssuingKeyRequest(
            Guid.NewGuid(),
            "TestKey",
            "did:prism:test123",
            "secp256k1",
            "privateKeyTest",
            "publicKeyTest",
            "publicKeyPart2Test");

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be("The tenant does not exist in the database. The IssuingKey cannot be created.");
    }

    [Theory]
    [InlineData("", "did:prism:test", "secp256k1", "pub", "priv")]
    [InlineData("TestKey", "", "secp256k1", "pub", "priv")]
    [InlineData("TestKey", "did:prism:test", "", "pub", "priv")]
    [InlineData("TestKey", "did:prism:test", "secp256k1", "", "priv")]
    [InlineData("TestKey", "did:prism:test", "secp256k1", "pub", "")]
    public async Task Handle_WithEmptyValues_ShouldCreateSuccessfully(
        string name, string did, string keyType, string publicKey, string privateKey)
    {
        // Arrange
        var tenantResult = await _createTenantHandler.Handle(new CreateTenantRequest("TestTenant"), CancellationToken.None);
        tenantResult.IsSuccess.Should().BeTrue();
        var tenantId = tenantResult.Value;

        var request = new CreateIssuingKeyRequest(
            tenantId,
            name,
            did,
            keyType,
            privateKey,
            publicKey,
            null);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(name);
        result.Value.Did.Should().Be(did);
        result.Value.KeyType.Should().Be(keyType);
        result.Value.PublicKey.Should().Be(publicKey);
        result.Value.PrivateKey.Should().Be(privateKey);
    }

    [Fact]
    public async Task Handle_MultipleKeysForSameTenant_ShouldSucceed()
    {
        // Arrange
        var tenantResult = await _createTenantHandler.Handle(new CreateTenantRequest("TestTenant"), CancellationToken.None);
        tenantResult.IsSuccess.Should().BeTrue();
        var tenantId = tenantResult.Value;

        var request1 = new CreateIssuingKeyRequest(
            tenantId,
            "TestKey1",
            "did:prism:test1",
            "secp256k1",
            "privateKey1",
            "publicKey1",
            null);

        var request2 = new CreateIssuingKeyRequest(
            tenantId,
            "TestKey2",
            "did:prism:test2",
            "secp256k1",
            "privateKey2",
            "publicKey2",
            null);

        // Act
        var result1 = await _handler.Handle(request1, CancellationToken.None);
        var result2 = await _handler.Handle(request2, CancellationToken.None);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();

        var issuingKeys = await _dataContext.IssuingKeys
            .Where(i => i.TenantEntityId == tenantId)
            .ToListAsync();

        issuingKeys.Should().HaveCount(2);
        issuingKeys.Should().Contain(k => k.Name == "TestKey1" && k.Did == "did:prism:test1");
        issuingKeys.Should().Contain(k => k.Name == "TestKey2" && k.Did == "did:prism:test2");
    }

    // [Fact]
    // public async Task Handle_WithLongValues_ShouldSucceed()
    // {
    //     // Arrange
    //     var tenantResult = await _createTenantHandler.Handle(new CreateTenantRequest("TestTenant"), CancellationToken.None);
    //     tenantResult.IsSuccess.Should().BeTrue();
    //     var tenantId = tenantResult.Value;
    //
    //     var longString = new string('x', 1000);
    //     var request = new CreateIssuingKeyRequest(
    //         tenantId,
    //         longString,
    //         longString,
    //         longString,
    //         longString,
    //         longString);
    //
    //     // Act
    //     var result = await _handler.Handle(request, CancellationToken.None);
    //
    //     // Assert
    //     result.IsSuccess.Should().BeTrue();
    //     result.Value.Should().NotBeNull();
    //     result.Value.Name.Should().Be(longString);
    //     result.Value.Did.Should().Be(longString);
    //     result.Value.KeyType.Should().Be(longString);
    //     result.Value.PublicKey.Should().Be(longString);
    //     result.Value.PrivateKey.Should().Be(longString);
    // }
}