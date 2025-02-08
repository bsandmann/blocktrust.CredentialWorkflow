namespace Blocktrust.CredentialWorkflow.Core.Tests.DIDCommTests
{
    using Blocktrust.CredentialWorkflow.Core;
    using Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.GetPeerDidById;
    using Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.SavePeerDID;
    using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.CreateTenant;
    using Blocktrust.CredentialWorkflow.Core.Tests;
    using FluentAssertions;
    using Xunit;

    public class GetPeerDidByIdHandlerTests : TestSetup
    {
        private readonly DataContext _dataContext;
        private readonly GetPeerDidByIdHandler _getPeerDidByIdHandler;
        private readonly CreateTenantHandler _createTenantHandler;
        private readonly SavePeerDIDHandler _savePeerDIDHandler;

        public GetPeerDidByIdHandlerTests(TransactionalTestDatabaseFixture fixture) : base(fixture)
        {
            _dataContext = fixture.CreateContext();
            _getPeerDidByIdHandler = new GetPeerDidByIdHandler(_dataContext);
            _createTenantHandler = new CreateTenantHandler(_dataContext);
            _savePeerDIDHandler = new SavePeerDIDHandler(_dataContext);
        }

        [Fact]
        public async Task Handle_ValidPeerDidEntityId_ShouldReturnPeerDid()
        {
            // Arrange
            // First create a tenant
            var tenantResult = await _createTenantHandler.Handle(new CreateTenantRequest("TestTenant"), CancellationToken.None);
            tenantResult.IsSuccess.Should().BeTrue();
            var tenantId = tenantResult.Value;

            // Then save a PeerDID
            var peerDidResult = await _savePeerDIDHandler.Handle(
                new SavePeerDIDRequest(tenantId, "TestPeerDID", "peerDidTest123"),
                CancellationToken.None);

            peerDidResult.IsSuccess.Should().BeTrue();
            var savedPeerDid = peerDidResult.Value;

            // Prepare the get request
            var getRequest = new GetPeerDidByIdRequest(savedPeerDid.PeerDIDEntityId);

            // Act
            var result = await _getPeerDidByIdHandler.Handle(getRequest, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue("we expect to find an existing PeerDID by the valid ID");
            result.Value.Should().NotBeNull();
            result.Value.PeerDIDEntityId.Should().Be(savedPeerDid.PeerDIDEntityId);
            result.Value.Name.Should().Be("TestPeerDID");
            result.Value.PeerDID.Should().Be("peerDidTest123");
        }

        [Fact]
        public async Task Handle_InvalidPeerDidEntityId_ShouldReturnFailure()
        {
            // Arrange
            var invalidPeerDidId = Guid.NewGuid();
            var getRequest = new GetPeerDidByIdRequest(invalidPeerDidId);

            // Act
            var result = await _getPeerDidByIdHandler.Handle(getRequest, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainSingle()
                .Which.Message.Should().Be($"PeerDID with ID '{invalidPeerDidId}' not found.");
        }
    }
}
