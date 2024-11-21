using Blocktrust.CredentialWorkflow.Core.Domain.Models.Did;
using Blocktrust.CredentialWorkflow.Core.Services.Interfaces;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Blocktrust.CredentialWorkflow.Core.Services;

public class DidResolutionService : IDidResolutionService
{
    private readonly ILogger<DidResolutionService> _logger;

    public DidResolutionService(ILogger<DidResolutionService> logger)
    {
        _logger = logger;
    }

    public async Task<Result<DidDocument>> ResolveDid(string did)
    {
        try
        {
            // TODO: Implement actual DID resolution
            return Result.Ok(new DidDocument());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve DID: {Did}", did);
            return Result.Fail<DidDocument>("Failed to resolve DID: " + ex.Message);
        }
    }
}