//
// namespace Blocktrust.CredentialWorkflow.Core.Tests;
//
// using Blocktrust.CredentialWorkflow.Core.Entities.Tenant;
// using Commands.Tenant.RemoveIdentusAgent;
// using FluentAssertions;
// using FluentResults.Extensions.FluentAssertions;
// using Microsoft.EntityFrameworkCore;
//
// public partial class TestSetup
// {
//     [Fact]
//     public async Task RemoveIdentusFromTenant_ForExistingIdentusAgent_ShouldSucceed()
//     {
//         // Arrange
//         var tenant = new TenantEntity
//         {
//             Name = "TestTenant",
//             CreatedUtc = DateTime.UtcNow
//         };
//         await _context.TenantEntities.AddAsync(tenant);
//
//         var identusAgent = new IdentusAgent
//         {
//             Name = "TestIdentusAgent",
//             Uri = new Uri("https://test.identus.com"),
//             ApiKey = "testApiKey123",
//             TenantId = tenant.TenantEntityId
//         };
//         await _context.IdentusAgents.AddAsync(identusAgent);
//         await _context.SaveChangesAsync();
//
//         var request = new RemoveIdentusAgentRequest(tenant.TenantEntityId, identusAgent.IdentusAgentId);
//         var handler = new RemoveIdentusAgentHandler(_context);
//
//         // Act
//         var result = await handler.Handle(request, CancellationToken.None);
//
//         // Assert
//         result.Should().BeSuccess();
//
//         // Verify the IdentusAgent was actually removed from the database
//         var identusAgentInDb = await _context.IdentusAgents
//             .FirstOrDefaultAsync(i => i.IdentusAgentId == identusAgent.IdentusAgentId);
//         identusAgentInDb.Should().BeNull();
//     }
//
//     [Fact]
//     public async Task RemoveIdentusFromTenant_ForNonExistentIdentusAgent_ShouldFail()
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
//         var nonExistentIdentusAgentId = Guid.NewGuid();
//         var request = new RemoveIdentusAgentRequest(tenant.TenantEntityId, nonExistentIdentusAgentId);
//         var handler = new RemoveIdentusAgentHandler(_context);
//
//         // Act
//         var result = await handler.Handle(request, CancellationToken.None);
//
//         // Assert
//         result.Should().BeFailure();
//         result.Errors.Should().Contain(e => e.Message.Contains($"IdentusAgent with ID {nonExistentIdentusAgentId} not found"));
//     }
//
//     [Fact]
//     public async Task RemoveIdentusFromTenant_ForWrongTenant_ShouldFail()
//     {
//         // Arrange
//         var tenant1 = new TenantEntity { Name = "Tenant1", CreatedUtc = DateTime.UtcNow };
//         var tenant2 = new TenantEntity { Name = "Tenant2", CreatedUtc = DateTime.UtcNow };
//         await _context.TenantEntities.AddRangeAsync(tenant1, tenant2);
//
//         var identusAgent = new IdentusAgent
//         {
//             Name = "TestIdentusAgent",
//             Uri = new Uri("https://test.identus.com"),
//             ApiKey = "testApiKey123",
//             TenantId = tenant1.TenantEntityId
//         };
//         await _context.IdentusAgents.AddAsync(identusAgent);
//         await _context.SaveChangesAsync();
//
//         var request = new RemoveIdentusAgentRequest(tenant2.TenantEntityId, identusAgent.IdentusAgentId);
//         var handler = new RemoveIdentusAgentHandler(_context);
//
//         // Act
//         var result = await handler.Handle(request, CancellationToken.None);
//
//         // Assert
//         result.Should().BeFailure();
//         result.Errors.Should().Contain(e => e.Message.Contains($"IdentusAgent with ID {identusAgent.IdentusAgentId} not found for Tenant with ID {tenant2.TenantEntityId}"));
//
//         // Verify the IdentusAgent still exists in the database
//         var identusAgentInDb = await _context.IdentusAgents
//             .FirstOrDefaultAsync(i => i.IdentusAgentId == identusAgent.IdentusAgentId);
//         identusAgentInDb.Should().NotBeNull();
//     }
// }