namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow;

using Domain.Common;
using Domain.Credential;
using Domain.ProcessFlow.Actions;
using Domain.ProcessFlow.Actions.Issuance;
using Domain.ProcessFlow.Triggers;
using Domain.Workflow;
using Entities.Outcome;
using FluentResults;
using IssueCredentials.IssueW3cCredential.CreateW3cCredential;
using IssueCredentials.IssueW3cCredential.SignW3cCredential;
using MediatR;
using Org.BouncyCastle.Bcpg.Sig;
using Tenant.GetIssuingKeys;
using Tenant.GetIssungKeyById;
using Tenant.GetPrivateIssuingKeyByDid;

public class ExecuteWorkflowHandler : IRequestHandler<ExecuteWorkflowRequest, Result<Guid>>
{
    private readonly IMediator _mediator;

    public ExecuteWorkflowHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Result<Guid>> Handle(ExecuteWorkflowRequest request, CancellationToken cancellationToken)
    {
        var workflow = request.ActionOutcome.Workflow;
        var executionContextString = request.ActionOutcome.ExecutionContext;
        ExecutionContext executionContext = BuildExecutionContext(workflow, executionContextString);


        // TODO ensure the correct order of actions
        foreach (var action in workflow!.ProcessFlow.Actions)
        {
            var actionId = action.Key;
            var actionType = action.Value.Type;
            var actionInput = action.Value.Input;

            if (actionType == EActionType.IssueW3CCredential)
            {
                var input = (IssueW3cCredential)actionInput;
                var subjectDid = await GetParameterFromExecutionContext(input.SubjectDid, executionContext);
                if (subjectDid == null)
                {
                    return Result.Fail("The subject DID is not provided.");
                }

                var issuerDid = await GetParameterFromExecutionContext(input.IssuerDid, executionContext);
                if (issuerDid == null)
                {
                    return Result.Fail("The issuer DID is not provided.");
                }


                var createW3CCredentialRequest = new CreateW3cCredentialRequest(
                    issuerDid: issuerDid,
                    subjectDid: subjectDid,
                    additionalSubjectData: GetClaimsFromExecutionContext(input.Claims, executionContext),
                    validFrom: null,
                    expirationDate: null
                );
                var createW3CCredentialResult = await _mediator.Send(createW3CCredentialRequest, cancellationToken);
                if (createW3CCredentialResult.IsFailed)
                {
                    return Result.Fail("The W3C credential could not be created.");
                }

                var issuignKeyResult = await _mediator.Send(new GetPrivateIssuingKeyByDidRequest(issuerDid), cancellationToken);
                if (issuignKeyResult.IsFailed)
                {
                    return Result.Fail("The private key for the issuer DID could not be found.");
                }

                var privatekeyResult = HexStringToByteArray(issuignKeyResult.Value);
                if(privatekeyResult.IsFailed)
                {
                    return privatekeyResult.ToResult();
                }

                var signedCredentialRequest = new SignW3cCredentialRequest(
                        credential: createW3CCredentialResult.Value,
                        issuerDid: issuerDid,
                        privateKey: privatekeyResult.Value);
                var signedCredentialResult = await _mediator.Send(signedCredentialRequest, cancellationToken);
                if (signedCredentialResult.IsFailed)
                {
                    return signedCredentialResult.ToResult();
                }

                // TODO save the signed credential in the output
            }
            else
            {
                return Result.Fail($"The action type {actionType} is not supported.");
            }
        }

        return Result.Ok(request.ActionOutcome.OutcomeId);
    }


    private ExecutionContext BuildExecutionContext(Workflow workflow, string? executionContextString)
    {
        var trigger = workflow?.ProcessFlow?.Triggers.First().Value;
        if (trigger is null)
        {
            return new ExecutionContext(workflow!.TenantId);
        }

        if (trigger.Type == ETriggerType.HttpRequest && executionContextString is not null)
        {
            return ExecutionContext.FromSimplifiedHttpContext(workflow.TenantId, executionContextString);
        }

        return new ExecutionContext(workflow!.TenantId);
    }

    private async Task<string?> GetParameterFromExecutionContext(ParameterReference parameterReference, ExecutionContext executionContext)
    {
        if (parameterReference.Source == ParameterSource.TriggerInput)
        {
            if (executionContext.InputContext is null)
            {
                return null;
            }

            var exists = executionContext.InputContext.TryGetValue(parameterReference.Path.ToLowerInvariant(), out var value);
            if (!exists)
            {
                return null;
            }

            return value;
        }
        else if (parameterReference.Source == ParameterSource.AppSettings)
        {
            // TODO currently the only thing saved in the AppSettings (don't confuse it with the appsettings.json) is the IssuingKeys

            var issuingKeys = await _mediator.Send(new GetIssuingKeysRequest(executionContext.TenantId));
            if (issuingKeys.IsFailed)
            {
                return null;
            }

            foreach (var issuingKey in issuingKeys.Value)
            {
                var keyId = Guid.Parse(parameterReference.Path);
                if (issuingKey.IssuingKeyId.Equals(keyId)) ;
                {
                    return issuingKey.Did;
                }
            }
        }

        return null;
    }

    private Dictionary<string, object>? GetClaimsFromExecutionContext(Dictionary<string, ClaimValue> inputClaims, ExecutionContext executionContext)
    {
        var claims = new Dictionary<string, object>();
        foreach (var inputClaim in inputClaims)
        {
            var key = inputClaim.Key;
            if (inputClaim.Value.Type == ClaimValueType.Static)
            {
                claims.TryAdd(key, inputClaim.Value.Value);
            }
            else if (inputClaim.Value.Type == ClaimValueType.TriggerProperty)
            {
                if (executionContext.InputContext is null)
                {
                    continue;
                }

                if (inputClaim.Value.ParameterReference is null)
                {
                    continue;
                }

                var exists = executionContext.InputContext.TryGetValue(inputClaim.Value.ParameterReference.Path.ToLowerInvariant(), out var value);
                if (!exists || value is null)
                {
                    continue;
                }

                claims.TryAdd(key, value);
            }
            else
            {
                continue;
            }
        }

        return claims;
    }

    // TODO refactor this to be placed somewhere where we actually save the private keys

    public static Result<byte[]> HexStringToByteArray(string hex)
    {
        if (hex.Length % 2 != 0)
        {
            return Result.Fail("Hex string must have an even number of characters.");
        }

        var byteCount = hex.Length / 2;
        var bytes = new byte[byteCount];

        for (int i = 0; i < byteCount; i++)
        {
            // Grab two characters from the string
            string twoChars = hex.Substring(i * 2, 2);
            // Convert the two characters into a byte
            bytes[i] = Convert.ToByte(twoChars, 16);
        }

        return Result.Ok(bytes);
    }
}