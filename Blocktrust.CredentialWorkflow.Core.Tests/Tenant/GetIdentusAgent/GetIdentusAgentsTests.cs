namespace Blocktrust.CredentialWorkflow.Core.Tests;

using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.GetIdentusAgents;
using Blocktrust.CredentialWorkflow.Core.Entities.Tenant;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;

public partial class TestSetup
{
    
    [Fact]
    public async Task GetIdentusAgents_ForExistingTenantWithAgents_ShouldSucceed()
    {
        // Arrange
        var tenant = new TenantEntity
        {
            Name = "TestTenant",
            CreatedUtc = DateTime.UtcNow,
            IdentusAgents = new List<IdentusAgent>
            {
                new() { Name = "Agent1", Uri = new Uri("https://agent1.com"), ApiKey = "key1" },
                new() { Name = "Agent2", Uri = new Uri("https://agent2.com"), ApiKey = "key2" }
            }
        };
        await _context.TenantEntities.AddAsync(tenant);
        await _context.SaveChangesAsync();

        var handler = new GetIdentusAgentsHandler(_context);
        var request = new GetIdentusAgentsRequest(tenant.TenantEntityId);
        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(a => a.Name == "Agent1" && a.Uri == new Uri("https://agent1.com") && a.ApiKey == "key1");
        result.Value.Should().Contain(a => a.Name == "Agent2" && a.Uri == new Uri("https://agent2.com") && a.ApiKey == "key2");
    }

    [Fact]
    public async Task GetIdentusAgents_ForExistingTenantWithoutAgents_ShouldReturnEmptyList()
    {
        // Arrange
        var tenant = new TenantEntity
        {
            Name = "EmptyTenant",
            CreatedUtc = DateTime.UtcNow
        };
        await _context.TenantEntities.AddAsync(tenant);
        await _context.SaveChangesAsync();

        var handler = new GetIdentusAgentsHandler(_context);
        var request = new GetIdentusAgentsRequest(tenant.TenantEntityId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetIdentusAgents_ForNonExistentTenant_ShouldFail()
    {
        // Arrange
        var nonExistentTenantId = Guid.NewGuid();
        var handler = new GetIdentusAgentsHandler(_context);
        var request = new GetIdentusAgentsRequest(nonExistentTenantId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeFailure();
        result.Errors.Should().Contain(e => e.Message.Contains($"Tenant with ID {nonExistentTenantId} not found"));
    }
}