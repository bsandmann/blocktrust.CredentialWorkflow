using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Action;
using Blocktrust.CredentialWorkflow.Core.Services.Interfaces;
using FluentResults;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Blocktrust.CredentialWorkflow.Core.Domain.Handlers.Actions;

public class CredentialIssuanceActionHandler : IActionHandler
{
    private readonly ICredentialService _credentialService;
    private readonly ILogger<CredentialIssuanceActionHandler> _logger;

    public CredentialIssuanceActionHandler(
        ICredentialService credentialService,
        ILogger<CredentialIssuanceActionHandler> logger)
    {
        _credentialService = credentialService;
        _logger = logger;
    }

    public async Task<Result<ActionResult>> ExecuteAsync(
        ActionInput input,
        WorkflowContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            var typedInput = (ActionInputCredentialIssuance)input;
            
            var issuanceResult = await _credentialService.IssueCredential(
                typedInput.SubjectDid,
                typedInput.IssuerDid,
                JsonSerializer.Serialize(typedInput.Claims));

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