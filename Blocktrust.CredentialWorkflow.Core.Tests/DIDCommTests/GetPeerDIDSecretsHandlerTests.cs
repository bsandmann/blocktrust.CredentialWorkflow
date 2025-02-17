using Blocktrust.Common.Models.DidDoc;
using Blocktrust.Common.Models.Secrets;
using Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.GetPeerDIDSecrets;
using Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.SavePeerDIDSecrets;
using FluentAssertions;

namespace Blocktrust.CredentialWorkflow.Core.Tests.DIDCommTests
{
    public class GetPeerDIDSecretsHandlerTests : TestSetup
    {
        private readonly DataContext _dataContext;
        private readonly GetPeerDIDSecretsHandler _getHandler;
        private readonly SavePeerDIDSecretsHandler _saveHandler;

        public GetPeerDIDSecretsHandlerTests(TransactionalTestDatabaseFixture fixture) : base(fixture)
        {
            _dataContext = fixture.CreateContext();
            _getHandler = new GetPeerDIDSecretsHandler(_dataContext);
            _saveHandler = new SavePeerDIDSecretsHandler(_dataContext);
        }

        [Fact]
        public async Task Handle_MultipleKids_ShouldReturnAllExistingSecrets()
        {
            // Arrange: Create multiple secrets
            var kid1 = "did:example:123#key1";
            var kid2 = "did:example:123#key2";

            await CreateSecret(kid1, "{\"kty\":\"EC\",\"crv\":\"secp256k1\",\"x\":\"abc\",\"y\":\"123\"}");
            await CreateSecret(kid2, "{\"kty\":\"EC\",\"crv\":\"secp256k1\",\"x\":\"def\",\"y\":\"456\"}");

            var request = new GetPeerDIDSecretsRequest(new List<string> { kid1, kid2 });

            // Act
            var result = await _getHandler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(2, "both secrets exist in the database");

            // Check presence of kid1 secret
            var secretForKid1 = result.Value.SingleOrDefault(x => x.Kid == kid1);
            secretForKid1.Should().NotBeNull();
            secretForKid1!.VerificationMaterial.Value.Should().Contain("abc");

            // Check presence of kid2 secret
            var secretForKid2 = result.Value.SingleOrDefault(x => x.Kid == kid2);
            secretForKid2.Should().NotBeNull();
            secretForKid2!.VerificationMaterial.Value.Should().Contain("def");
        }

        [Fact]
        public async Task Handle_NoMatchingKids_ShouldReturnEmptyList()
        {
            // Arrange: No secrets exist in the database for these KIDs
            var request = new GetPeerDIDSecretsRequest(new List<string> { "invalidKid1", "invalidKid2" });

            // Act
            var result = await _getHandler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Should().BeEmpty("no secrets exist for the provided KIDs");
        }

        [Fact]
        public async Task Handle_EmptyKidList_ShouldReturnEmptyList()
        {
            // Arrange: Some logic might treat an empty request as a valid scenario 
            // or potentially an invalid request in your domain. 
            // Adjust as needed if you throw an error for an empty list.
            var request = new GetPeerDIDSecretsRequest(new List<string>());

            // Act
            var result = await _getHandler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEmpty("no KIDs were requested, so no secrets should be returned");
        }

        /// <summary>
        /// Helper method to create a secret in the database via the SavePeerDIDSecretsHandler.
        /// </summary>
        private async Task CreateSecret(string kid, string jsonValue)
        {
            var secret = new Secret
            {
                Type = VerificationMethodType.JsonWebKey2020,
                VerificationMaterial = new VerificationMaterial
                {
                    Format = VerificationMaterialFormat.Jwk,
                    Value = jsonValue
                },
                Kid = kid
            };

            var request = new SavePeerDIDSecretRequest(kid, secret);
            var saveResult = await _saveHandler.Handle(request, CancellationToken.None);
            saveResult.IsSuccess.Should().BeTrue("the secret creation should succeed");
        }
    }
}
