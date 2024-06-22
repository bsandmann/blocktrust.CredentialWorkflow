namespace Blocktrust.CredentialWorkflow.Core.Tests;

using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.DeleteTenant;
using Blocktrust.CredentialWorkflow.Core.Entities.Tenant;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using Microsoft.EntityFrameworkCore;

public partial class TestSetup
{
    [Fact]
    public async Task DeleteTenant_ExistingTenant_ShouldSucceed()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenant = new TenantEntity
        {
            TenantEntityId = tenantId,
            Name = "TestTenant",
            CreatedUtc = DateTime.UtcNow
        };
        await _context.TenantEntities.AddAsync(tenant);
        await _context.SaveChangesAsync();

        var request = new DeleteTenantRequest(tenantId);

        // Act
        var result = await _deleteTenantHandler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();

        // Verify the tenant was actually deleted from the database
        var deletedTenant = await _context.TenantEntities.FindAsync(tenantId);
        deletedTenant.Should().BeNull();
    }
}