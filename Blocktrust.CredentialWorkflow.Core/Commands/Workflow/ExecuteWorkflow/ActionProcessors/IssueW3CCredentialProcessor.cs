using Blocktrust.Common.Converter;
using Blocktrust.CredentialWorkflow.Core.Commands.IssueCredentials.IssueW3cCredential.CreateW3cCredential;
using Blocktrust.CredentialWorkflow.Core.Commands.IssueCredentials.IssueW3cCredential.SignW3cCredential;
using Blocktrust.CredentialWorkflow.Core.Commands.Tenant.GetPrivateIssuingKeyByDid;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Issuance;
using FluentResults;
using MediatR;
using Action = Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Action;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow.ActionProcessors;

using Action = Action;

public class IssueW3CCredentialProcessor : IActionProcessor
{
    private readonly IMediator _mediator;

    public IssueW3CCredentialProcessor(IMediator mediator)
    {
        _mediator = mediator;
    }

    public EActionType ActionType => EActionType.IssueW3CCredential;

    public async Task<Result> ProcessAsync(Action action, ActionOutcome actionOutcome, ActionProcessingContext context)
    {
        var input = (IssueW3cCredential)action.Input;

        var subjectDid = await ParameterResolver.GetParameterFromExecutionContext(
            input.SubjectDid, context.ExecutionContext, context.Workflow, context.ActionOutcomes, ActionType, _mediator);
        if (subjectDid == null)
        {
            var errorMessage = "The subject DID is not provided in the execution context parameters.";
            actionOutcome.FinishOutcomeWithFailure(errorMessage);
            return Result.Fail(errorMessage);
        }

        var issuerDid = await ParameterResolver.GetParameterFromExecutionContext(
            input.IssuerDid, context.ExecutionContext, context.Workflow, context.ActionOutcomes, ActionType, _mediator);
        if (issuerDid == null)
        {
            var errorMessage = "The issuer DID is not provided.";
            actionOutcome.FinishOutcomeWithFailure(errorMessage);
            return Result.Fail(errorMessage);
        }

        var claims = ParameterResolver.GetClaimsFromExecutionContext(input.Claims, context.ExecutionContext);
        if (claims == null)
        {
            var errorMessage = "No claims provided for the credential.";
            actionOutcome.FinishOutcomeWithFailure(errorMessage);
            return Result.Fail(errorMessage);
        }
        
        // Handle the ValidUntil property
        DateTimeOffset? expirationDate = null;
        if (input.ValidUntil.HasValue)
        {
            // Validate that the date is not in the past
            if (input.ValidUntil.Value.Date < DateTime.Today)
            {
                var errorMessage = "The expiration date cannot be in the past.";
                actionOutcome.FinishOutcomeWithFailure(errorMessage);
                return Result.Fail(errorMessage);
            }

            // Set expiration to the end of the specified day (23:59:59.999)
            var endOfDay = input.ValidUntil.Value.Date.AddDays(1).AddTicks(-1);
            expirationDate = new DateTimeOffset(endOfDay);
        }

        var createW3CCredentialRequest = new CreateW3cCredentialRequest(
            issuerDid: issuerDid,
            subjectDid: subjectDid,
            additionalSubjectData: claims,
            validFrom: null, // Will default to current time in the handler
            expirationDate: expirationDate
        );

        var createW3CCredentialResult = await _mediator.Send(createW3CCredentialRequest, context.CancellationToken);
        if (createW3CCredentialResult.IsFailed)
        {
            var errorMessage = "The W3C credential could not be created.";
            actionOutcome.FinishOutcomeWithFailure(errorMessage);
            return Result.Fail(errorMessage);
        }

        var issuingKeyResult = await _mediator.Send(new GetPrivateIssuingKeyByDidRequest(issuerDid), context.CancellationToken);
        if (issuingKeyResult.IsFailed)
        {
            var errorMessage = "The private key for the issuer DID could not be found.";
            actionOutcome.FinishOutcomeWithFailure(errorMessage);
            return Result.Fail(errorMessage);
        }

        byte[] privateKey;
        try
        {
            privateKey = Base64Url.Decode(issuingKeyResult.Value);
        }
        catch (FormatException ex)
        {
            var errorMessage = $"Invalid private key format: {ex.Message}";
            actionOutcome.FinishOutcomeWithFailure(errorMessage);
            return Result.Fail(errorMessage);
        }

        var signedCredentialRequest = new SignW3cCredentialRequest(
            credential: createW3CCredentialResult.Value,
            issuerDid: issuerDid,
            privateKey: privateKey
        );

        var signedCredentialResult = await _mediator.Send(signedCredentialRequest, context.CancellationToken);
        if (signedCredentialResult.IsFailed)
        {
            var errorMessage = signedCredentialResult.Errors.FirstOrDefault()?.Message ?? "The credential could not be signed.";
            actionOutcome.FinishOutcomeWithFailure(errorMessage);
            return Result.Fail(errorMessage);
        }

        actionOutcome.FinishOutcomeWithSuccess(signedCredentialResult.Value);
        return Result.Ok();
    }
}