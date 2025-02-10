namespace Blocktrust.CredentialWorkflow.Core.Tests.DIDCommTests
{
    using Blocktrust.CredentialWorkflow.Core;
    using Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.GetPeerDIDs;
    using Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.SavePeerDID;
    using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.CreateTenant;
    using Blocktrust.CredentialWorkflow.Core.Tests;
    using FluentAssertions;
    using Xunit;

    public class GetPeerDIDsHandlerTests : TestSetup
    {
        private readonly DataContext _dataContext;
        private readonly GetPeerDIDsHandler _getPeerDIDsHandler;
        private readonly CreateTenantHandler _createTenantHandler;
        private readonly SavePeerDIDHandler _savePeerDIDHandler;

        public GetPeerDIDsHandlerTests(TransactionalTestDatabaseFixture fixture) : base(fixture)
        {
            _dataContext = fixture.CreateContext();
            _getPeerDIDsHandler = new GetPeerDIDsHandler(_dataContext);
            _createTenantHandler = new CreateTenantHandler(_dataContext);
            _savePeerDIDHandler = new SavePeerDIDHandler(_dataContext);
        }

        [Fact]
        public async Task Handle_TenantWithMultiplePeerDIDs_ReturnsList()
        {
            // Arrange
            var tenantResult = await _createTenantHandler.Handle(new CreateTenantRequest("TestTenant"), CancellationToken.None);
            tenantResult.IsSuccess.Should().BeTrue();
            var tenantId = tenantResult.Value;

            // Create multiple PeerDIDs for the tenant
            await _savePeerDIDHandler.Handle(
                new SavePeerDIDRequest(tenantId, "PeerDID1", "peerDid123"), 
                CancellationToken.None);
            await _savePeerDIDHandler.Handle(
                new SavePeerDIDRequest(tenantId, "PeerDID2", "peerDid456"), 
                CancellationToken.None);

            var getRequest = new GetPeerDIDsRequest(tenantId);

            // Act
            var result = await _getPeerDIDsHandler.Handle(getRequest, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(2);
            result.Value.Should().ContainSingle(d => d.Name == "PeerDID1" && d.PeerDID == "peerDid123");
            result.Value.Should().ContainSingle(d => d.Name == "PeerDID2" && d.PeerDID == "peerDid456");
        }

        [Fact]
        public async Task Handle_TenantWithNoPeerDIDs_ReturnsEmptyList()
        {
            // Arrange
            var tenantResult = await _createTenantHandler.Handle(new CreateTenantRequest("EmptyTenant"), CancellationToken.None);
            tenantResult.IsSuccess.Should().BeTrue();
            var tenantId = tenantResult.Value;

            // No PeerDIDs created for this tenant
            var getRequest = new GetPeerDIDsRequest(tenantId);

            // Act
            var result = await _getPeerDIDsHandler.Handle(getRequest, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Should().BeEmpty("no PeerDIDs exist for this tenant");
        }

        [Fact]
        public async Task Handle_NonExistentTenant_ReturnsEmptyList()
        {
            // Arrange
            // Pass in a random GUID that doesn't match any tenant
            var invalidTenantId = Guid.NewGuid();
            var getRequest = new GetPeerDIDsRequest(invalidTenantId);

            // Act
            var result = await _getPeerDIDsHandler.Handle(getRequest, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue("the handler simply returns an empty list if no PeerDIDs are found");
            result.Value.Should().NotBeNull();
            result.Value.Should().BeEmpty();
        }
    }
}
