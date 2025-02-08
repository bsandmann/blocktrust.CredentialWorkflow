namespace Blocktrust.CredentialWorkflow.Core.Tests.DIDCommTests
{
    using Blocktrust.Common.Models.DidDoc;
    using Blocktrust.Common.Models.Secrets;
    using Blocktrust.CredentialWorkflow.Core;
    using Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.SavePeerDIDSecrets;
    using Blocktrust.CredentialWorkflow.Core.Tests;
    using FluentAssertions;
    using Xunit;

    public class SavePeerDIDSecretsHandlerTests : TestSetup
    {
        private readonly DataContext _dataContext;
        private readonly SavePeerDIDSecretsHandler _handler;

        public SavePeerDIDSecretsHandlerTests(TransactionalTestDatabaseFixture fixture) : base(fixture)
        {
            _dataContext = fixture.CreateContext();
            _handler = new SavePeerDIDSecretsHandler(_dataContext);
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldSavePeerDIDSecret()
        {
            // Arrange
            var secret = new Secret
            {
                Type = VerificationMethodType.JsonWebKey2020,
                VerificationMaterial = new VerificationMaterial
                {
                    Format = VerificationMaterialFormat.Jwk,
                    Value = "{\"kty\":\"EC\",\"crv\":\"secp256k1\",\"x\":\"abc\",\"y\":\"123\"}" // Example JSON
                }
            };

            var kid = "did:example:123#key-1";
            var request = new SavePeerDIDSecretRequest(kid, secret);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue("the request is valid, so saving should succeed");

            // Verify that the secret was saved in the database
            var savedSecret = _dataContext.PeerDIDSecrets.FirstOrDefault(x => x.Kid == kid);
            savedSecret.Should().NotBeNull("we expect an entry to be created in PeerDIDSecretEntities table");
            savedSecret!.Kid.Should().Be(kid);
            savedSecret.Value.Should().Be(secret.VerificationMaterial.Value);
            savedSecret.VerificationMaterialFormat.Should().Be((int)secret.VerificationMaterial.Format);
            savedSecret.VerificationMethodType.Should().Be((int)secret.Type);
        }

        [Fact]
        public async Task Handle_NullSecret_ShouldThrowOrFail()
        {
            // Arrange
            var request = new SavePeerDIDSecretRequest("someKid", null!);

            // Act
            Func<Task> act = async () => await _handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NullReferenceException>("the Secret is null and code does not guard against it");
        }
    }
}
