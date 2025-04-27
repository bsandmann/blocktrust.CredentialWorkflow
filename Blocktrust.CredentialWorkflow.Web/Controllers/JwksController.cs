using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.GetTenantInformation;
using Blocktrust.CredentialWorkflow.Core.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blocktrust.CredentialWorkflow.Web.Controllers;

/// <summary>
/// Controller for providing JWKS (JSON Web Key Set) endpoints
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("{tenantId}")]
public class JwksController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public JwksController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    /// <summary>
    /// Returns the public keys of a tenant in JWKS format
    /// </summary>
    /// <param name="tenantId">The ID of the tenant</param>
    /// <returns>A JWKS document containing the tenant's public keys for JWT verification</returns>
    [HttpGet(".well-known/jwks.json")]
    [Produces("application/json")]
    [ResponseCache(Duration = 3600)] // Cache for 1 hour
    public async Task<IActionResult> GetJwks(Guid tenantId)
    {
        try
        {
            // Fetch tenant information
            var result = await _mediator.Send(new GetTenantInformationRequest(tenantId));
            
            if (result.IsFailed)
            {
                return NotFound("Tenant not found");
            }
            
            var tenant = result.Value.Tenant;
            
            if (string.IsNullOrEmpty(tenant.JwtPublicKey))
            {
                return NotFound("No JWT public key available for this tenant");
            }
            
            // Generate JWKS using the JwtKeyGeneratorService
            string jwksJson = JwtKeyGeneratorService.GenerateJwksJsonFromXmlPublicKey(
                tenant.JwtPublicKey,
                $"tenant-{tenantId}", // Use tenant ID as key ID
                JwtKeyGeneratorService.GetAlgorithmIdentifier());
            
            // Return as raw JSON
            return Content(jwksJson, "application/json");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while retrieving JWKS: {ex.Message}");
        }
    }
}