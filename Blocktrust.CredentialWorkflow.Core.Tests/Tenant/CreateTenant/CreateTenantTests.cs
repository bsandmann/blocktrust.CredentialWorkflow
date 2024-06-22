namespace Blocktrust.CredentialWorkflow.Core.Tests;

using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.CreateTenant;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;

public partial class TestSetup
{
    [Fact]
    public async Task CreateTenant_with_valid_name()
    {
        // Arrange
        var request = new CreateTenantRequest(name: "BlockTrustTenant");

        // Act
        var result = await _createTenantHandler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeSuccess();
        result.Value.Should().NotBe(Guid.Empty);
    }
    
    [Fact]
    public async Task CreateTenant_with_empty_name_should_fail()
    {
        // Arrange
        var request = new CreateTenantRequest(name: "");

        // Act
        var result = await _createTenantHandler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeFailure();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be("The tenant name must be provided");
    }
}