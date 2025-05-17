using Blocktrust.CredentialWorkflow.Core.Commands.VerifyCredentials.VerifyW3cCredentials.CheckExpiry;
using Blocktrust.CredentialWorkflow.Core.Commands.VerifyCredentials.VerifyW3cCredentials.CheckRevocation;
using Blocktrust.CredentialWorkflow.Core.Commands.VerifyCredentials.VerifyW3cCredentials.CheckSignature;
using Blocktrust.CredentialWorkflow.Core.Domain.Verification;
using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.VerifyCredentials.VerifyW3cCredentials.VerifyW3cCredential;

using Services;

public class VerifyW3CCredentialHandler : IRequestHandler<VerifyW3CCredentialRequest, Result<CredentialVerificationResult>>
{
    private readonly IMediator _mediator;
    private readonly CredentialParser _credentialParser;

    public VerifyW3CCredentialHandler(IMediator mediator, CredentialParser credentialParser)
    {
        _mediator = mediator;
        _credentialParser = credentialParser;
    }

    public async Task<Result<CredentialVerificationResult>> Handle(VerifyW3CCredentialRequest request, CancellationToken cancellationToken)
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
            if (request.VerifySignature)
            {
                var signatureResult = await _mediator.Send(new CheckSignatureRequest(credential), cancellationToken);
                if (signatureResult.IsFailed)
                {
                    return Result.Fail<CredentialVerificationResult>($"Failed to verify signature: {signatureResult.Errors.FirstOrDefault()?.Message}");
                }

                verificationResult.SignatureValid = signatureResult.Value;
            }

            // Check expiry
            if (request.VerifyExpiry)
            {
                var expiryResult = await _mediator.Send(new CheckExpiryRequest(credential), cancellationToken);
                if (expiryResult.IsFailed)
                {
                    return Result.Fail<CredentialVerificationResult>("Failed to check expiry status");
                }

                verificationResult.IsExpired = expiryResult.Value;
            }

            // Check revocation
            if (request.VerifyRevocationStatus)
            {
                var revocationResult = await _mediator.Send(new CheckRevocationRequest(credential), cancellationToken);
                if (revocationResult.IsFailed)
                {
                    return Result.Fail<CredentialVerificationResult>("Failed to check revocation status");
                }

                verificationResult.IsRevoked = revocationResult.Value;
            }

            return Result.Ok(verificationResult);
        }
        catch (Exception ex)
        {
            return Result.Fail<CredentialVerificationResult>(new Error("VerifyCredentials failed").CausedBy(ex));
        }
    }
}