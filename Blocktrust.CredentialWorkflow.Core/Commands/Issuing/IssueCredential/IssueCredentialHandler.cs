using System.Text.Json;
using Blocktrust.CredentialWorkflow.Core.Crypto;
using Blocktrust.CredentialWorkflow.Core.Prism;
using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Issuing.IssueCredential;

public class IssueCredentialHandler : IRequestHandler<IssueCredentialRequest, Result<string>>
{
    private readonly IEcService _ecService;

    public IssueCredentialHandler(IEcService ecService)
    {
        _ecService = ecService;
    }

    public async Task<Result<string>> Handle(IssueCredentialRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Create the header
            var header = new
            {
                alg = "ES256K",
                typ = "JWT"
            };
            var headerJson = JsonSerializer.Serialize(header);
            var headerBase64 = PrismEncoding.ByteArrayToBase64(PrismEncoding.Utf8StringToByteArray(headerJson));

            // Create the payload
            var payload = new
            {
                iss = request.IssuerDid,
                sub = request.Credential.CredentialSubjects?.FirstOrDefault()?.Id,
                nbf = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                exp = DateTimeOffset.UtcNow.AddYears(5).ToUnixTimeSeconds(), // 5 years expiry
                vc = request.Credential
            };
            var payloadJson = JsonSerializer.Serialize(payload);
            var payloadBase64 = PrismEncoding.ByteArrayToBase64(PrismEncoding.Utf8StringToByteArray(payloadJson));

            // Create signing input
            var signingInput = $"{headerBase64}.{payloadBase64}";
            var dataToSign = PrismEncoding.Utf8StringToByteArray(signingInput);

            // Sign the data
            var signature = _ecService.SignDataWithoutDER(dataToSign, request.PrivateKey);
            var signatureBase64 = PrismEncoding.ByteArrayToBase64(signature);

            // Create the final JWT
            var jwt = $"{headerBase64}.{payloadBase64}.{signatureBase64}";

            return Result.Ok(jwt);
        }
        catch (Exception ex)
        {
            return Result.Fail<string>($"Failed to sign credential: {ex.Message}");
        }
    }
}