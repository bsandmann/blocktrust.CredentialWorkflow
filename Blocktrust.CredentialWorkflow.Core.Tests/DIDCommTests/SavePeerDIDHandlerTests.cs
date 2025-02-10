namespace Blocktrust.CredentialWorkflow.Core.Tests.DIDCommTests
{
    using Blocktrust.CredentialWorkflow.Core;
    using Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.SavePeerDID;
    using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.CreateTenant;
    using Blocktrust.CredentialWorkflow.Core.Tests;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using Xunit;

    public class SavePeerDIDHandlerTests : TestSetup
    {
        private readonly DataContext _dataContext;
        private readonly SavePeerDIDHandler _handler;
        private readonly CreateTenantHandler _createTenantHandler;

        public SavePeerDIDHandlerTests(TransactionalTestDatabaseFixture fixture) : base(fixture)
        {
            _dataContext = fixture.CreateContext();
            _handler = new SavePeerDIDHandler(_dataContext);
            _createTenantHandler = new CreateTenantHandler(_dataContext);
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldSavePeerDID()
        {
            // Arrange
            var tenantResult = await _createTenantHandler.Handle(new CreateTenantRequest("TestTenant"), CancellationToken.None);
            tenantResult.IsSuccess.Should().BeTrue();
            var tenantId = tenantResult.Value;

            var request = new SavePeerDIDRequest(
                tenantId,
                "TestPeerDID",
                "peerDidTest123");

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue("the tenant exists and the PeerDID should be created successfully");
            result.Value.Should().NotBeNull();
            result.Value.Name.Should().Be("TestPeerDID");
            result.Value.PeerDID.Should().Be("peerDidTest123");

            // Verify database state
            var peerDIDEntity = await _dataContext.PeerDIDEntities
                .FirstOrDefaultAsync(p => p.TenantEntityId == tenantId, CancellationToken.None);

            peerDIDEntity.Should().NotBeNull("a new record should be persisted in the database");
            peerDIDEntity!.Name.Should().Be("TestPeerDID");
            peerDIDEntity.PeerDID.Should().Be("peerDidTest123");
        }

        [Fact]
        public async Task Handle_InvalidTenant_ShouldFail()
        {
            // Arrange
            // We pass an invalid tenant Id that doesn't exist in the database
            var invalidTenantId = Guid.NewGuid();
            var request = new SavePeerDIDRequest(
                invalidTenantId,
                "TestPeerDID",
                "peerDidTest123");

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue("the request references a non-existing tenant");
            result.Errors.Should().ContainSingle()
                .Which.Message.Should()
                .Be("The tenant does not exist in the database. The PeerDID cannot be created.");
        }
    }
}
