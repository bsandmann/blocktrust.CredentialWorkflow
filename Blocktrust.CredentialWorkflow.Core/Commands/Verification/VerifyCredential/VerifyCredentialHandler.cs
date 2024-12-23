using FluentResults;
using MediatR;
using Blocktrust.CredentialWorkflow.Core.Commands.Verification.CheckSignature;
using Blocktrust.CredentialWorkflow.Core.Commands.Verification.CheckExpiry;
using Blocktrust.CredentialWorkflow.Core.Commands.Verification.CheckRevocation;
using Blocktrust.CredentialWorkflow.Core.Domain.Verification;
using Blocktrust.CredentialWorkflow.Core.Services;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Verification.VerifyCredential;

public class VerifyCredentialHandler : IRequestHandler<VerifyCredentialRequest, Result<CredentialVerificationResult>>
{
    private readonly IMediator _mediator;
    private readonly CredentialParser _credentialParser;

    public VerifyCredentialHandler(IMediator mediator, CredentialParser credentialParser)
    {
        _mediator = mediator;
        _credentialParser = credentialParser;
    }

    public async Task<Result<CredentialVerificationResult>> Handle(VerifyCredentialRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Parse the credential
            var parsedCredentialResult = _credentialParser.ParseCredential(request.Credential);
            if (parsedCredentialResult.IsFailed)
            {
                return Result.Fail<CredentialVerificationResult>("Failed to parse credential");
            }

            var credential = parsedCredentialResult.Value;
            var verificationResult = new CredentialVerificationResult();

            // Check signature
            var signatureResult = await _mediator.Send(new CheckSignatureRequest(credential), cancellationToken);
            if (signatureResult.IsFailed)
            {
                return Result.Fail<CredentialVerificationResult>("Failed to verify signature");
            }
            verificationResult.SignatureValid = signatureResult.Value;

            // Check expiry
            var expiryResult = await _mediator.Send(new CheckExpiryRequest(credential), cancellationToken);
            if (expiryResult.IsFailed)
            {
                return Result.Fail<CredentialVerificationResult>("Failed to check expiry status");
            }
            verificationResult.IsExpired = expiryResult.Value;

            // Check revocation
            var revocationResult = await _mediator.Send(new CheckRevocationRequest(credential), cancellationToken);
            if (revocationResult.IsFailed)
            {
                return Result.Fail<CredentialVerificationResult>("Failed to check revocation status");
            }
            verificationResult.IsRevoked = revocationResult.Value;

            return Result.Ok(verificationResult);
        }
        catch (Exception ex)
        {
            return Result.Fail<CredentialVerificationResult>(new Error("Verification failed").CausedBy(ex));
        }
    }
}