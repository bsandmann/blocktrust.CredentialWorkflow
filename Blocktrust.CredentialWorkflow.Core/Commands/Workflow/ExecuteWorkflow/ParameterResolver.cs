using Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.GetPeerDIDs;
using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.GetIssuingKeys;
using Blocktrust.CredentialWorkflow.Core.Domain.Common;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;
using MediatR;
using ExecutionContext = Blocktrust.CredentialWorkflow.Core.Domain.Common.ExecutionContext;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow;

using ExecutionContext = ExecutionContext;

public static class ParameterResolver
{
    public static async Task<string?> GetParameterFromExecutionContext(
        ParameterReference parameterReference,
        ExecutionContext executionContext,
        Domain.Workflow.Workflow workflow,
        List<ActionOutcome> actionOutcomes,
        EActionType? actionType = null,
        IMediator? mediator = null)
    {
        switch (parameterReference.Source)
        {
            case ParameterSource.Static:
                return parameterReference.Path;

            case ParameterSource.TriggerInput:
                if (executionContext.InputContext is null)
                {
                    return null;
                }

                executionContext.InputContext.TryGetValue(parameterReference.Path.ToLowerInvariant(), out var value);
                return value;

            case ParameterSource.AppSettings:
                if (actionType == EActionType.IssueW3CCredential && mediator != null)
                {
                    var issuingKeysResult = await mediator.Send(new GetIssuingKeysRequest(executionContext.TenantId));
                    if (issuingKeysResult.IsFailed || issuingKeysResult.Value is null || issuingKeysResult.Value.Count == 0)
                    {
                        return null;
                    }

                    if (parameterReference.Path.Equals("DefaultIssuerDid", StringComparison.CurrentCultureIgnoreCase))
                    {
                        return issuingKeysResult.Value.First().Did;
                    }

                    if (Guid.TryParse(parameterReference.Path, out var keyId))
                    {
                        var issuingKey = issuingKeysResult.Value.FirstOrDefault(k => k.IssuingKeyId == keyId);
                        return issuingKey?.Did;
                    }
                }
                else if (actionType == EActionType.DIDComm && mediator != null)
                {
                    var peerDidsResult = await mediator.Send(new GetPeerDIDsRequest(executionContext.TenantId));
                    if (peerDidsResult.IsFailed || peerDidsResult.Value is null || peerDidsResult.Value.Count == 0)
                    {
                        return null;
                    }

                    if (parameterReference.Path.Equals("DefaultSenderDid", StringComparison.CurrentCultureIgnoreCase))
                    {
                        return peerDidsResult.Value.First().PeerDID;
                    }

                    if (Guid.TryParse(parameterReference.Path, out var keyId))
                    {
                        var peerDid = peerDidsResult.Value.FirstOrDefault(p => p.PeerDIDEntityId == keyId);
                        return peerDid?.PeerDID;
                    }
                }

                return null;

            case ParameterSource.ActionOutcome:
                var actionId = parameterReference.ActionId;
                if (actionId is not null)
                {
                    var actionOutcome = actionOutcomes.FirstOrDefault(a => a.ActionId == actionId);
                    return actionOutcome?.OutcomeJson;
                }

                var actionIdIsParsabel = Guid.TryParse(parameterReference.Path, out var pathActionId);
                if (actionIdIsParsabel)
                {
                    var actionOutcome = actionOutcomes.FirstOrDefault(a => a.ActionId == pathActionId);
                    return actionOutcome?.OutcomeJson;
                }

                return null;
            default:
                return null;
        }
    }

    public static Dictionary<string, object>? GetClaimsFromExecutionContext(
        Dictionary<string, ClaimValue> inputClaims,
        ExecutionContext executionContext)
    {
        var claims = new Dictionary<string, object>();
        foreach (var claim in inputClaims)
        {
            var key = claim.Key;
            if (claim.Value.Type == ClaimValueType.Static)
            {
                claims[key] = claim.Value.Value;
            }
            else if (claim.Value.Type == ClaimValueType.TriggerProperty && executionContext.InputContext != null)
            {
                if (claim.Value.ParameterReference != null && executionContext.InputContext.TryGetValue(claim.Value.ParameterReference.Path.ToLowerInvariant(), out var value))
                {
                    claims[key] = value;
                }
            }
        }

        return claims.Any() ? claims : null;
    }
}