using Blocktrust.CredentialWorkflow.Core.Commands.VerifyCredentials.VerifyW3cCredentials.VerifyW3cCredential;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Verification;
using FluentResults;
using MediatR;
using Action = Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Action;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow.ActionProcessors;

using Action = Action;

public class VerifyW3CCredentialProcessor : IActionProcessor
{
    private readonly IMediator _mediator;

    public VerifyW3CCredentialProcessor(IMediator mediator)
    {
        _mediator = mediator;
    }

    public EActionType ActionType => EActionType.VerifyW3CCredential;

    public async Task<Result> ProcessAsync(Action action, ActionOutcome actionOutcome, ActionProcessingContext context)
    {
        var input = (VerifyW3cCredential)action.Input;

        var credentialStr = await ParameterResolver.GetParameterFromExecutionContext(
            input.CredentialReference, context.ExecutionContext, context.Workflow, context.ActionOutcomes, ActionType, _mediator);
        if (string.IsNullOrWhiteSpace(credentialStr))
        {
            var errorMessage = "No credential found in the execution context to verify.";
            actionOutcome.FinishOutcomeWithFailure(errorMessage);
            return Result.Fail(errorMessage);
        }

        var verifyRequest = new VerifyW3CCredentialRequest(
            credentialStr,
            input.CheckSignature,
            input.CheckExpiry,
            input.CheckRevocationStatus,
            input.CheckSchema,
            input.CheckTrustRegistry
        );

        var verifyResult = await _mediator.Send(verifyRequest, context.CancellationToken);
        if (verifyResult.IsFailed)
        {
            var errorMessage = verifyResult.Errors.FirstOrDefault()?.Message ?? "Verification failed.";
            actionOutcome.FinishOutcomeWithFailure(errorMessage);
            return Result.Fail(errorMessage);
        }

        var verificationResult = verifyResult.Value;
        actionOutcome.FinishOutcomeWithSuccess($"Credential verified. IsValid={verificationResult.IsValid}. Credential checked: {credentialStr}");
        return Result.Ok();
    }
}