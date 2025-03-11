using System.Text.Json;
using System.Text.Json.Serialization;
using Blocktrust.CredentialWorkflow.Core.Crypto;
using Blocktrust.CredentialWorkflow.Core.Domain.Credential;
using Blocktrust.CredentialWorkflow.Core.Prism;
using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.IssueCredentials.IssueW3cCredential.SignW3cCredential;

public class SignW3cCredentialHandler : IRequestHandler<SignW3cCredentialRequest, Result<string>>
{
    private readonly IEcService _ecService;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
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

            Credential cleanCredential = new Credential
            {
                CredentialContext = request.Credential.CredentialContext,
                Type = request.Credential.Type,
                CredentialSubjects = request.Credential.CredentialSubjects,
                CredentialIssuer = request.Credential.CredentialIssuer,
                Id = request.Credential.Id,
                IssuanceDate = request.Credential.IssuanceDate,
                ExpirationDate = request.Credential.ExpirationDate,
                ValidFrom = request.Credential.ValidFrom,
                ValidUntil = request.Credential.ValidUntil,
                Proofs = request.Credential.Proofs,
                CredentialStatus = request.Credential.CredentialStatus,
                CredentialSchemas = request.Credential.CredentialSchemas,
                RefreshServices = request.Credential.RefreshServices,
                TermsOfUses = request.Credential.TermsOfUses,
                Evidences = request.Credential.Evidences,
                AdditionalData = request.Credential.AdditionalData,
                JwtParsingArtefact = request.Credential.JwtParsingArtefact,
                SerializationOption = request.Credential.SerializationOption,
                DataModelType = request.Credential.DataModelType
            };

            var payload = new Dictionary<string, object>
            {
                { "iss", request.IssuerDid },
                { "sub", request.Credential.CredentialSubjects?.FirstOrDefault()?.Id },
                { "vc", cleanCredential }
            };
            
            // Add not-before (nbf) claim from credential's ValidFrom date if available
            if (request.Credential.ValidFrom.HasValue)
            {
                payload["nbf"] = new DateTimeOffset(request.Credential.ValidFrom.Value).ToUnixTimeSeconds();
            }
            else
            {
                // Default to current time if ValidFrom is not specified
                payload["nbf"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
            
            // Add expiration (exp) claim only if ExpirationDate is specified in the credential
            if (request.Credential.ExpirationDate.HasValue)
            {
                payload["exp"] = new DateTimeOffset(request.Credential.ExpirationDate.Value).ToUnixTimeSeconds();
            }

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