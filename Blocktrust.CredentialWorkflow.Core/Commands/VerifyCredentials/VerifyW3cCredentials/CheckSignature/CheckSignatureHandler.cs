using Blocktrust.CredentialWorkflow.Core.Crypto;
using Blocktrust.CredentialWorkflow.Core.Prism;
using Blocktrust.CredentialWorkflow.Core.Services.DIDPrism;
using FluentResults;
using MediatR;
using DidPrismResolverClient; // Or whichever namespace your PrismDidClient is in.

namespace Blocktrust.CredentialWorkflow.Core.Commands.VerifyCredentials.VerifyW3cCredentials.CheckSignature
{
    public class CheckSignatureHandler : IRequestHandler<CheckSignatureRequest, Result<bool>>
    {
        private readonly ExtractPrismPubKeyFromLongFormDid _extractPrismPubKey;
        private readonly IEcService _ecService;
        private readonly PrismDidClient _prismDidClient;

        public CheckSignatureHandler(
            ExtractPrismPubKeyFromLongFormDid extractPrismPubKey,
            IEcService ecService,
            PrismDidClient prismDidClient)
        {
            _extractPrismPubKey = extractPrismPubKey;
            _ecService = ecService;
            _prismDidClient = prismDidClient;
        }

        public async Task<Result<bool>> Handle(CheckSignatureRequest request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate the presence of required fields
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

                // 1) Check that the DID is a did:prism
                string did = request.Credential.CredentialIssuer.IssuerId.ToString();
                if (!did.Contains("did:prism:", StringComparison.InvariantCultureIgnoreCase))
                {
                    return Result.Fail("Invalid DID method (not did:prism:...)");
                }

                // 2) Distinguish long-form vs. short-form
                var colonSplit = did.Split(':', StringSplitOptions.RemoveEmptyEntries);
                bool isLongForm = (colonSplit.Length == 4);
                bool isShortForm = (colonSplit.Length == 3);

                if (!isLongForm && !isShortForm)
                {
                    return Result.Fail("Invalid DID structure: must be short or long form of did:prism.");
                }

                // 3) Retrieve the public key
                byte[] publicKey;

                if (isLongForm)
                {
                    // a) Extract from the long form directly
                    var extractedPublicKey = _extractPrismPubKey.Extract(did);

                    // b) Attempt to resolve from the remote API
                    var resolutionResult = await TryResolvePublicKeyAsync(did, cancellationToken);

                    if (resolutionResult.IsSuccess)
                    {
                        // Override the local extraction with the DID document result
                        publicKey = resolutionResult.PublicKey;
                    }
                    else
                    {
                        if (resolutionResult.IsDeactivated)
                        {
                            // Deactivated => fail
                            return Result.Fail("DID is deactivated (per resolution API).");
                        }
                        else
                        {
                            // Unreachable or other error => keep local extraction
                            publicKey = extractedPublicKey;
                        }
                    }
                }
                else
                {
                    // Short form => must always resolve from the API
                    var resolutionResult = await TryResolvePublicKeyAsync(did, cancellationToken);

                    if (!resolutionResult.IsSuccess)
                    {
                        // If unreachable or error => fail
                        if (resolutionResult.IsDeactivated)
                        {
                            return Result.Fail("DID is deactivated (per resolution API).");
                        }

                        return Result.Fail($"Failed to resolve short-form DID: {resolutionResult.ErrorMessage}");
                    }

                    publicKey = resolutionResult.PublicKey;
                }

                // 4) Perform the final signature verification
                byte[] signatureBytes = PrismEncoding.Base64ToByteArray(
                    request.Credential.JwtParsingArtefact.JwtSignature
                        .Replace('-', '+')
                        .Replace('_', '/')
                );

                string signInInput =
                    $"{PrismEncoding.ByteArrayToBase64(PrismEncoding.Utf8StringToByteArray(request.Credential.HeaderJson))}" +
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

        /// <summary>
        /// Calls the Prism DID resolution API to retrieve the DID Document and extracts the public key.
        /// Returns a small record capturing success/failure, a deactivation flag, and the extracted key.
        /// </summary>
        private async Task<DidResolutionKeyResult> TryResolvePublicKeyAsync(string did, CancellationToken cancellationToken)
        {
            try
            {
                // Attempt to resolve
                var didDocument = await _prismDidClient.ResolveDidDocumentAsync(did, cancellationToken: cancellationToken);

                if (didDocument == null)
                {
                    return DidResolutionKeyResult.Failure("Empty DID document returned from server.");
                }

                // Check for an assertionMethod
                if (didDocument.AssertionMethod == null || didDocument.AssertionMethod.Count == 0)
                {
                    return DidResolutionKeyResult.Failure("No issuer key found in DID document.");
                }
                if (didDocument.AssertionMethod.Count > 1)
                {
                    return DidResolutionKeyResult.Failure("Multiple issuer keys found in DID document.");
                }

                var assertionMethod = didDocument.AssertionMethod[0].Split('#');
                if (assertionMethod.Length != 2)
                {
                    return DidResolutionKeyResult.Failure("Invalid AssertionMethod key syntax.");
                }

                var keyPart = assertionMethod[1];
                var issuingKey = didDocument.VerificationMethod.FirstOrDefault(
                    p => p.Id == didDocument.AssertionMethod[0]
                         || p.Id == keyPart
                         || p.Id == "#" + keyPart);

                if (issuingKey?.PublicKeyJwk == null)
                {
                    return DidResolutionKeyResult.Failure("Issuer key not found in DID document.");
                }

                if (!issuingKey.PublicKeyJwk.KeyType.Equals("EC", StringComparison.OrdinalIgnoreCase))
                {
                    return DidResolutionKeyResult.Failure("Unable to extract public key (non-EC key) from DID document.");
                }

                // Parse out x & y
                var xKey = issuingKey.PublicKeyJwk.X;
                var yKey = issuingKey.PublicKeyJwk.Y;

                if (string.IsNullOrEmpty(xKey))
                {
                    return DidResolutionKeyResult.Failure("Missing 'x' coordinate in public key JWK.");
                }

                byte[] finalKey;
                if (string.IsNullOrEmpty(yKey))
                {
                    // If 'y' is not provided (maybe compressed key)
                    finalKey = PrismEncoding.HexToByteArray(xKey);
                }
                else
                {
                    var xBytes = PrismEncoding.Base64ToByteArray(xKey);
                    var yBytes = PrismEncoding.Base64ToByteArray(yKey);

                    finalKey = PrismEncoding.HexToByteArray(
                        PrismEncoding.PublicKeyPairByteArraysToHex(xBytes, yBytes)
                    );
                }

                return DidResolutionKeyResult.Success(finalKey);
            }
            catch (PrismDidResolutionException ex)
            {
                // Inspect ex.Message for specific status codes, e.g. 410 Gone => deactivated
                if (ex.Message.Contains("410")) // or parse for the exact status code if you prefer
                {
                    return DidResolutionKeyResult.Deactivated("DID is deactivated (HTTP 410).");
                }

                // Otherwise, treat as a generic unreachable/failure
                return DidResolutionKeyResult.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                // Some other network or JSON parsing error
                return DidResolutionKeyResult.Failure($"Unexpected error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Same result record as in your other handler: captures success, deactivation flag, error message, and public key bytes.
    /// </summary>
    public record DidResolutionKeyResult
    {
        public bool IsSuccess { get; init; }
        public bool IsDeactivated { get; init; }
        public string ErrorMessage { get; init; } = string.Empty;
        public byte[] PublicKey { get; init; } = Array.Empty<byte>();

        public static DidResolutionKeyResult Success(byte[] key) => new()
        {
            IsSuccess = true,
            IsDeactivated = false,
            PublicKey = key
        };

        public static DidResolutionKeyResult Failure(string error) => new()
        {
            IsSuccess = false,
            IsDeactivated = false,
            ErrorMessage = error
        };

        public static DidResolutionKeyResult Deactivated(string error) => new()
        {
            IsSuccess = false,
            IsDeactivated = true,
            ErrorMessage = error
        };
    }
}
