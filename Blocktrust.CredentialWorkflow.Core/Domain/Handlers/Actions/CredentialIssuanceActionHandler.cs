using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Action;
using Blocktrust.CredentialWorkflow.Core.Services.Interfaces;
using FluentResults;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Blocktrust.CredentialWorkflow.Core.Domain.Handlers.Actions;

public class CredentialIssuanceActionHandler : IActionHandler
{
    private readonly ICredentialService _credentialService;
    private readonly ILogger<CredentialIssuanceActionHandler> _logger;
    private readonly IConfiguration _configuration;

    public CredentialIssuanceActionHandler(
        ICredentialService credentialService,
        ILogger<CredentialIssuanceActionHandler> logger,
        IConfiguration configuration)
    {
        _credentialService = credentialService;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<Result<ActionResult>> ExecuteAsync(
        ActionInput input,
        WorkflowContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            var typedInput = (ActionInputCredentialIssuance)input;
            
            var subjectDid = typedInput.SubjectDid.ResolveValue(context, _configuration);
            var issuerDid = typedInput.IssuerDid.ResolveValue(context, _configuration);

            if (string.IsNullOrEmpty(subjectDid) || string.IsNullOrEmpty(issuerDid))
            {
                return Result.Fail<ActionResult>("Required DIDs not available");
            }

            // Resolve claims
            var resolvedClaims = new Dictionary<string, string>();
            foreach (var claim in typedInput.Claims)
            {
                var value = claim.Value.Type == ClaimValueType.Static 
                    ? claim.Value.Value 
                    : claim.Value.ParameterReference?.ResolveValue(context, _configuration);
                
                if (value != null)
                {
                    resolvedClaims[claim.Key] = value;
                }
            }

            var issuanceResult = await _credentialService.IssueCredential(
                subjectDid,
                issuerDid,
                JsonSerializer.Serialize(resolvedClaims));

            if (issuanceResult.IsFailed)
            {
                return Result.Fail<ActionResult>(issuanceResult.Errors);
            }

            return Result.Ok(new ActionResult
            {
                Success = true,
                OutputJson = JsonSerializer.Serialize(new { credential = issuanceResult.Value })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute credential issuance");
            return Result.Fail<ActionResult>("Credential issuance failed: " + ex.Message);
        }
    }
}