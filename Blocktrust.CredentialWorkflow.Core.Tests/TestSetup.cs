namespace Blocktrust.CredentialWorkflow.Core.Tests;

using System;
using Commands.Tenant.CreateTenant;
using Commands.Tenant.DeleteTenant;
using MediatR;
using Moq;
using Xunit;

[Collection("TransactionalTests")]
public partial class TestSetup : IDisposable
{
    private TransactionalTestDatabaseFixture Fixture { get; }

    private readonly DataContext _context;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly CreateTenantHandler _createTenantHandler;
    private readonly DeleteTenantHandler _deleteTenantHandler;

    public TestSetup(TransactionalTestDatabaseFixture fixture)
    {
        this.Fixture = fixture;
        this._context = this.Fixture.CreateContext();

        this._mediatorMock = new Mock<IMediator>();

        this._createTenantHandler = new CreateTenantHandler(_context);
        this._deleteTenantHandler = new DeleteTenantHandler(_context, _mediatorMock.Object);

        // this._mediatorMock.Setup(p => p.Send(It.IsAny<DeletePoolRequest>(), It.IsAny<CancellationToken>()))
        //     .Returns(async (DeletePoolRequest request, CancellationToken token) => await this._deletePoolHandler.Handle(request, token));
    }

    public void Dispose()
    {
        this.Fixture.Cleanup();
    }
}