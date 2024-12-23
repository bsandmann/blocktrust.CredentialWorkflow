using FluentResults;
using MediatR;
using Blocktrust.CredentialWorkflow.Core.Services.DIDPrism;
using Blocktrust.CredentialWorkflow.Core.Crypto;
using Blocktrust.CredentialWorkflow.Core.Prism;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Verification.CheckSignature;

public class CheckSignatureHandler : IRequestHandler<CheckSignatureRequest, Result<bool>>
{
    private readonly ExtractPrismPubKeyFromLongFormDid _extractPrismPubKey;
    private readonly IEcService _ecService;

    public CheckSignatureHandler(ExtractPrismPubKeyFromLongFormDid extractPrismPubKey, IEcService ecService)
    {
        _extractPrismPubKey = extractPrismPubKey;
        _ecService = ecService;
    }

    public async Task<Result<bool>> Handle(CheckSignatureRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.Credential.JwtParsingArtefact?.JwtSignature is null)
            {
                return Result.Fail("No JWT signature found in credential");
            }

            if (request.Credential.CredentialIssuer?.IssuerId is null)
            {
                return Result.Fail("No issuer found in credential");
            }

            if (request.Credential.HeaderJson is null || request.Credential.PayloadJson is null)
            {
                return Result.Fail("Missing JWT header or payload");
            }

            var publicKey = _extractPrismPubKey.Extract(request.Credential.CredentialIssuer.IssuerId.ToString());

            byte[] signatureBytes = PrismEncoding.Base64ToByteArray(
                request.Credential.JwtParsingArtefact.JwtSignature.Replace('-', '+').Replace('_', '/'));

            string signInInput = $"{PrismEncoding.ByteArrayToBase64(PrismEncoding.Utf8StringToByteArray(request.Credential.HeaderJson))}" +
                               $".{PrismEncoding.ByteArrayToBase64(PrismEncoding.Utf8StringToByteArray(request.Credential.PayloadJson))}";

            byte[] dataToVerify = PrismEncoding.Utf8StringToByteArray(signInInput);

            bool isValid = _ecService.VerifyDataWithoutDER(dataToVerify, signatureBytes, publicKey);

            return Result.Ok(isValid);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error during signature verification: {ex.Message}");
        }
    }
}