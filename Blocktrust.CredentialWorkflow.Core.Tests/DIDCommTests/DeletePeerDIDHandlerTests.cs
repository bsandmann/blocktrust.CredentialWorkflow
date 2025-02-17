namespace Blocktrust.CredentialWorkflow.Core.Tests.DIDCommTests
{
    using Blocktrust.CredentialWorkflow.Core;
    using Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.DeletePeerDID;
    using Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.SavePeerDID;
    using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.CreateTenant;
    using Blocktrust.CredentialWorkflow.Core.Tests;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using Xunit;

    public class DeletePeerDIDHandlerTests : TestSetup
    {
        private readonly DataContext _dataContext;
        private readonly DeletePeerDIDHandler _deletePeerDIDHandler;
        private readonly CreateTenantHandler _createTenantHandler;
        private readonly SavePeerDIDHandler _savePeerDIDHandler;

        public DeletePeerDIDHandlerTests(TransactionalTestDatabaseFixture fixture) : base(fixture)
        {
            _dataContext = fixture.CreateContext();
            _deletePeerDIDHandler = new DeletePeerDIDHandler(_dataContext);
            _createTenantHandler = new CreateTenantHandler(_dataContext);
            _savePeerDIDHandler = new SavePeerDIDHandler(_dataContext);
        }

        [Fact]
        public async Task Handle_ExistingPeerDID_ShouldDeleteAndReturnSuccess()
        {
            // Arrange
            // 1. Create a tenant.
            var tenantResult = await _createTenantHandler.Handle(new CreateTenantRequest("TestTenant"), CancellationToken.None);
            tenantResult.IsSuccess.Should().BeTrue();
            var tenantId = tenantResult.Value;

            // 2. Save a PeerDID for that tenant.
            var saveRequest = new SavePeerDIDRequest(
                tenantId,
                "TestPeerDID",
                "peerDidToDelete");
            var peerDidResult = await _savePeerDIDHandler.Handle(saveRequest, CancellationToken.None);
            peerDidResult.IsSuccess.Should().BeTrue();
            var peerDIDEntityId = peerDidResult.Value.PeerDIDEntityId;

            // Act
            // 3. Delete the PeerDID.
            var deleteRequest = new DeletePeerDIDRequest(peerDIDEntityId);
            var deleteResult = await _deletePeerDIDHandler.Handle(deleteRequest, CancellationToken.None);

            // Assert
            deleteResult.IsSuccess.Should().BeTrue("the PeerDID exists and should be deleted without errors");

            // Verify the entity has been removed from the database
            var peerDIDEntity = await _dataContext.PeerDIDEntities
                .FirstOrDefaultAsync(p => p.PeerDIDEntityId == peerDIDEntityId);
            peerDIDEntity.Should().BeNull("the PeerDID should have been deleted from the database");
        }

        [Fact]
        public async Task Handle_NonExistentPeerDID_ShouldReturnFailure()
        {
            // Arrange
            // Use a random GUID that doesn't exist in the database
            var invalidPeerDIDId = Guid.NewGuid();
            var deleteRequest = new DeletePeerDIDRequest(invalidPeerDIDId);

            // Act
            var deleteResult = await _deletePeerDIDHandler.Handle(deleteRequest, CancellationToken.None);

            // Assert
            deleteResult.IsFailed.Should().BeTrue("no PeerDID with that ID exists in the database");
            deleteResult.Errors.Should().ContainSingle()
                .Which.Message.Should().Be("The PeerDID does not exist in the database. It cannot be deleted.");
        }
    }
}
