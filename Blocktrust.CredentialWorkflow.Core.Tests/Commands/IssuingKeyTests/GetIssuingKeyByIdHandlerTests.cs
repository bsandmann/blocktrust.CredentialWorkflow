using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.CreateIssuingKey;
using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.CreateTenant;
using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.GetIssungKeyById;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;


namespace Blocktrust.CredentialWorkflow.Core.Tests.Commands.IssuingKeyTests;

public class GetIssuingKeyByIdHandlerTests : TestSetup
{
    private readonly GetIssuingKeyByIdHandler _handler;
    private readonly CreateIssuingKeyHandler _createIssuingKeyHandler;
    private readonly CreateTenantHandler _createTenantHandler;
    private readonly DataContext _dataContext;

    public GetIssuingKeyByIdHandlerTests(TransactionalTestDatabaseFixture fixture) : base(fixture)
    {
        _dataContext = fixture.CreateContext();
        _handler = new GetIssuingKeyByIdHandler(_dataContext);
        _createIssuingKeyHandler = new CreateIssuingKeyHandler(_dataContext);
        _createTenantHandler = new CreateTenantHandler(_dataContext);
    }

    [Fact]
    public async Task Handle_NonExistentKeyId_ShouldReturnFailure()
    {
        // Arrange
        var nonExistentKeyId = Guid.NewGuid();
        var request = new GetIssuingKeyByIdRequest(nonExistentKeyId);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be("Issuing key not found.");

        var keyInDb = await _dataContext.IssuingKeys.FindAsync(nonExistentKeyId);
        keyInDb.Should().BeNull();
    }

    [Fact]
    public async Task Handle_MultipleKeysForSameTenant_ShouldReturnCorrectKey()
    {
        // Arrange
        var tenantResult = await _createTenantHandler.Handle(
            new CreateTenantRequest("TestTenant"), 
            CancellationToken.None);
        tenantResult.IsSuccess.Should().BeTrue();
        var tenantId = tenantResult.Value;

        // Create multiple keys for the same tenant
        var key1Result = await _createIssuingKeyHandler.Handle(
            new CreateIssuingKeyRequest(
                tenantId,
                "TestKey1",
                "did:prism:test1",
                "secp256k1",
                "publicKey1",
                "privateKey1"),
            CancellationToken.None);

        var key2Result = await _createIssuingKeyHandler.Handle(
            new CreateIssuingKeyRequest(
                tenantId,
                "TestKey2",
                "did:prism:test2",
                "secp256k1",
                "publicKey2",
                "privateKey2"),
            CancellationToken.None);

        var key3Result = await _createIssuingKeyHandler.Handle(
            new CreateIssuingKeyRequest(
                tenantId,
                "TestKey3",
                "did:prism:test3",
                "secp256k1",
                "publicKey3",
                "privateKey3"),
            CancellationToken.None);

        // Verify all keys were created successfully
        key1Result.IsSuccess.Should().BeTrue();
        key2Result.IsSuccess.Should().BeTrue();
        key3Result.IsSuccess.Should().BeTrue();

        // Act - retrieve the second key
        var result = await _handler.Handle(
            new GetIssuingKeyByIdRequest(key2Result.Value.IssuingKeyId), 
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.IssuingKeyId.Should().Be(key2Result.Value.IssuingKeyId);
        result.Value.Name.Should().Be("TestKey2");
        result.Value.Did.Should().Be("did:prism:test2");

        // Verify all keys exist in database
        var keysInDb = await _dataContext.IssuingKeys
            .Where(k => k.TenantEntityId == tenantId)
            .ToListAsync();
        keysInDb.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_KeyDataIntegrity_ShouldPreserveAllFields()
    {
        // Arrange
        var tenantResult = await _createTenantHandler.Handle(
            new CreateTenantRequest("TestTenant"), 
            CancellationToken.None);
        tenantResult.IsSuccess.Should().BeTrue();

        // Create a key with specific test data
        var complexData = new
        {
            Name = "Complex Key Name with Spaces and Special Chars !@#$",
            Did = "did:prism:test:with:multiple:colons:and:special:chars:!@#$",
            KeyType = "secp256k1WithSpecialParams!@#$",
            PublicKey = new string('A', 1000), // Long public key
            PrivateKey = new string('B', 1000) // Long private key
        };

        var createKeyResult = await _createIssuingKeyHandler.Handle(
            new CreateIssuingKeyRequest(
                tenantResult.Value,
                complexData.Name,
                complexData.Did,
                complexData.KeyType,
                complexData.PublicKey,
                complexData.PrivateKey),
            CancellationToken.None);
        createKeyResult.IsSuccess.Should().BeTrue();

        // Act
        var result = await _handler.Handle(
            new GetIssuingKeyByIdRequest(createKeyResult.Value.IssuingKeyId), 
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        // Verify all fields maintain exact data integrity
        result.Value.Name.Should().Be(complexData.Name);
        result.Value.Did.Should().Be(complexData.Did);
        result.Value.KeyType.Should().Be(complexData.KeyType);
        result.Value.PublicKey.Should().Be(complexData.PublicKey);
        result.Value.PrivateKey.Should().Be(complexData.PrivateKey);

        // Verify length and content of long fields
        result.Value.PublicKey.Length.Should().Be(1000);
        result.Value.PrivateKey.Length.Should().Be(1000);

        // Verify data integrity in database
        var keyInDb = await _dataContext.IssuingKeys.FindAsync(result.Value.IssuingKeyId);
        keyInDb.Should().NotBeNull();
        keyInDb!.Name.Should().Be(complexData.Name);
        keyInDb.Did.Should().Be(complexData.Did);
        keyInDb.KeyType.Should().Be(complexData.KeyType);
        keyInDb.PublicKey.Should().Be(complexData.PublicKey);
        keyInDb.PrivateKey.Should().Be(complexData.PrivateKey);
    }

    [Fact]
    public async Task Handle_EmptyGuid_ShouldReturnFailure()
    {
        // Arrange
        var request = new GetIssuingKeyByIdRequest(Guid.Empty);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be("Issuing key not found.");
    }
}