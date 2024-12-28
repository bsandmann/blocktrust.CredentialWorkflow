using System.Text.Json;
using System.Text.Json.Serialization;
using Blocktrust.CredentialWorkflow.Core.Crypto;
using Blocktrust.CredentialWorkflow.Core.Prism;
using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.IssueCredentials.IssueW3cCredential;

public class SignW3cCredentialHandler : IRequestHandler<SignW3cCredentialRequest, Result<string>>
{
    private readonly IEcService _ecService;
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public SignW3cCredentialHandler(IEcService ecService)
    {
        _ecService = ecService;
    }

    public async Task<Result<string>> Handle(SignW3cCredentialRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var header = new Dictionary<string, string>
            {
                { "alg", "ES256K" },
                { "typ", "JWT" }
            };
            
            var headerJson = JsonSerializer.Serialize(header, SerializerOptions);
            var headerBase64 = PrismEncoding.ByteArrayToBase64(PrismEncoding.Utf8StringToByteArray(headerJson));

            // Clean credential for payload
            var cleanCredential = new 
            {
                request.Credential.CredentialContext,
                request.Credential.Type,
                Issuer = request.IssuerDid,
                request.Credential.IssuanceDate,
                request.Credential.ExpirationDate,
                request.Credential.ValidFrom,
                request.Credential.ValidUntil,
                CredentialSubject = request.Credential.CredentialSubjects?.FirstOrDefault(),
                request.Credential.Proofs,
                request.Credential.CredentialStatus,
                request.Credential.CredentialSchemas,
                request.Credential.RefreshServices,
                request.Credential.TermsOfUses,
                request.Credential.Evidences
            };

            var payload = new Dictionary<string, object>
            {
                { "iss", request.IssuerDid },
                { "sub", request.Credential.CredentialSubjects?.FirstOrDefault()?.Id },
                { "nbf", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                { "exp", DateTimeOffset.UtcNow.AddYears(5).ToUnixTimeSeconds() },
                { "vc", cleanCredential }
            };

            var payloadJson = JsonSerializer.Serialize(payload, SerializerOptions);
            var payloadBase64 = PrismEncoding.ByteArrayToBase64(PrismEncoding.Utf8StringToByteArray(payloadJson));

            var signingInput = $"{headerBase64}.{payloadBase64}";
            var dataToSign = PrismEncoding.Utf8StringToByteArray(signingInput);

            var signature = _ecService.SignDataWithoutDER(dataToSign, request.PrivateKey);
            var signatureBase64 = PrismEncoding.ByteArrayToBase64(signature);

            return Result.Ok($"{headerBase64}.{payloadBase64}.{signatureBase64}");
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to sign credential: {ex.Message}");
        }
    }
}