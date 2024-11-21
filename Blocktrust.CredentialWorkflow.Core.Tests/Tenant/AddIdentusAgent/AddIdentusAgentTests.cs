// namespace Blocktrust.CredentialWorkflow.Core.Tests;
//
// using Blocktrust.CredentialWorkflow.Core.Entities.Tenant;
// using Commands.Tenant.AddIdentusAgent;
// using FluentAssertions;
// using FluentResults.Extensions.FluentAssertions;
// using Microsoft.EntityFrameworkCore;
//
// public partial class TestSetup
// {
//     [Fact]
//     public async Task AddIdentusToTenant_ForExistingTenant_ShouldSucceed()
//     {
//         // Arrange
//         var tenant = new TenantEntity
//         {
//             Name = "TestTenant",
//             CreatedUtc = DateTime.UtcNow
//         };
//         await _context.TenantEntities.AddAsync(tenant);
//         await _context.SaveChangesAsync();
//
//         var request = new AddIdentusAgentRequest(
//             tenant.TenantEntityId,
//             "TestIdentusAgent",
//             new Uri("https://test.identus.com"),
//             "testApiKey123"
//         );
//         var handler = new AddIdentusAgentHandler(_context);
//
//         // Act
//         var result = await handler.Handle(request, CancellationToken.None);
//
//         // Assert
//         result.Should().BeSuccess();
//         result.Value.Should().NotBe(Guid.Empty);
//
//         // Verify the IdentusAgent was actually added to the database
//         var identusAgentInDb = await _context.IdentusAgents
//             .FirstOrDefaultAsync(i => i.TenantId == tenant.TenantEntityId);
//         identusAgentInDb.Should().NotBeNull();
//         identusAgentInDb!.Name.Should().Be("TestIdentusAgent");
//         identusAgentInDb.Uri.Should().Be(new Uri("https://test.identus.com"));
//         identusAgentInDb.ApiKey.Should().Be("testApiKey123");
//     }
//
//     [Fact]
//     public async Task AddMultipleIdentusAgents_ForSameTenant_ShouldSucceed()
//     {
//         // Arrange
//         var tenantId = Guid.NewGuid();
//         var tenant = new TenantEntity
//         {
//             TenantEntityId = tenantId,
//             Name = "TestTenant",
//             CreatedUtc = DateTime.UtcNow
//         };
//         await _context.TenantEntities.AddAsync(tenant);
//         await _context.SaveChangesAsync();
//
//         var handler = new AddIdentusAgentHandler(_context);
//
//         // Act
//         var result1 = await handler.Handle(new AddIdentusAgentRequest(tenantId, "Agent1", new Uri("https://agent1.com"), "key1"), CancellationToken.None);
//         var result2 = await handler.Handle(new AddIdentusAgentRequest(tenantId, "Agent2", new Uri("https://agent2.com"), "key2"), CancellationToken.None);
//         var result3 = await handler.Handle(new AddIdentusAgentRequest(tenantId, "Agent3", new Uri("https://agent3.com"), "key3"), CancellationToken.None);
//
//         // Assert
//         result1.Should().BeSuccess();
//         result2.Should().BeSuccess();
//         result3.Should().BeSuccess();
//
//         result1.Value.Should().NotBe(Guid.Empty);
//         result2.Value.Should().NotBe(Guid.Empty);
//         result3.Value.Should().NotBe(Guid.Empty);
//
//         result1.Value.Should().NotBe(result2.Value);
//         result2.Value.Should().NotBe(result3.Value);
//         result1.Value.Should().NotBe(result3.Value);
//
//         // Verify the IdentusAgents were actually added to the database
//         var identusAgentsInDb = await _context.IdentusAgents
//             .Where(i => i.TenantId == tenantId)
//             .ToListAsync();
//
//         identusAgentsInDb.Should().HaveCount(3);
//         identusAgentsInDb.Select(i => i.IdentusAgentId).Should().OnlyHaveUniqueItems();
//         identusAgentsInDb.Should().Contain(i => i.Name == "Agent1" && i.Uri == new Uri("https://agent1.com") && i.ApiKey == "key1");
//         identusAgentsInDb.Should().Contain(i => i.Name == "Agent2" && i.Uri == new Uri("https://agent2.com") && i.ApiKey == "key2");
//         identusAgentsInDb.Should().Contain(i => i.Name == "Agent3" && i.Uri == new Uri("https://agent3.com") && i.ApiKey == "key3");
//     }
// }