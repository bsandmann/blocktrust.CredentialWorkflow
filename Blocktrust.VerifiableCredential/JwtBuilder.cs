namespace Blocktrust.VerifiableCredential;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Converters;
using Common.JwtModels;
using FluentResults;
using Base64Url = Common.Base64Url;

public static class JwtBuilder
{
    /// <summary>
    /// Build a JWT out of a single or multiple credentials
    /// This method does not sign the JWT
    /// </summary>
    /// <param name="credentials">The presentations</param>
    /// <param name="alg">The algorithm used for signing (if the JWT will be signed) e.g. ES256K (for secp256k1 in PRISM) or EdDSA (for Ed25519VerificationKey2018 in Indy) </param>
    /// <param name="kid">kid MAY be used if there are multiple keys associated with the issuer of the JWT. e.g. did:prism:something#key-1</param>
    /// <param name="options">Set of options which can be left as default</param>
    /// <param name="additionalClaims">if the JWT should also contain additional data. Be sure not to overlap the keys with the pre-defined keys for a JWT</param>
    /// <returns></returns>
    public static Result<JwtModel> Build(List<VerifiableCredential> credentials, string? alg, string? kid, JwtBuilderOptions options, Dictionary<string, object>? additionalClaims = null)
    {
        if (options.BuildJwtWithoutSignature)
        {
            if (credentials.Select(p => p.Proofs).Any(p => p == null))
            {
                return Result.Fail("Cannot build a JWT without a signature if the credential has no proof");
            }
        }
        else
        {
            // If we are creating a signature (JWS) then we can omit the proof from the credential
            if (options.OmitProofFromCredentialOrPresentation)
            {
                credentials = credentials.Select(p => p with { Proofs = null }).ToList();
            }
        }

        var expirationDate = credentials.Select(p => p.ExpirationDate).Where(p => p != null).Min();
        var notValidBefore = credentials.Select(p => p.IssuanceDate).Where(p => p != null).Max();
        var validUntil = credentials.Select(p => p.ValidUntil).Where(p => p != null).Min();
        var validFrom = credentials.Select(p => p.ValidFrom).Where(p => p != null).Max();

        var startDate = notValidBefore ?? validFrom;
        var endDate = expirationDate ?? validUntil;

        if (startDate is not null && startDate.Value.Kind == DateTimeKind.Unspecified)
        {
            startDate = DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc);
        }

        if (endDate is not null && endDate.Value.Kind == DateTimeKind.Unspecified)
        {
            endDate = DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc);
        }

        var issuers = credentials.Select(p => p.CredentialIssuer?.IssuerId).ToHashSet().Distinct().ToList();
        if (issuers.Count() > 1)
        {
            return Result.Fail("Cannot build a JWT with multiple issuers. All issuers of the credentials must be the same.");
        }

        var jwtId = credentials.Select(p => p.Id).ToHashSet().Distinct().ToList();
        if (jwtId.Count() > 1)
        {
            return Result.Fail("Cannot build a JWT with multiple ids. All ids of the credentials must be the same.");
        }

        var subject = credentials.SelectMany(p => p.CredentialSubjects.Select(q => q.Id)).ToHashSet().Distinct().ToList();
        if (subject.Count() > 1)
        {
            return Result.Fail("Cannot build a JWT with multiple ids. All ids of the credentialsubject must be the same.");
        }

        var result = BuildInteral(credentials, null, options, startDate, endDate, issuers!.Single(), jwtId.FirstOrDefault(), subject.FirstOrDefault(), null, alg, kid, null, additionalClaims);
        if (result.IsFailed)
        {
            return result.ToResult();
        }

        return result.Value;
    }

    /// <summary>
    /// Build a JWT out of a single or multiple presentations
    /// This method does not sign the JWT
    /// </summary>
    /// <param name="presentations">The presentations</param>
    /// <param name="audience">MUST represent (i.e., identify) the intended audience of the verifiable presentation (i.e., the verifier intended by the presenting holder to receive and verify the verifiable presentation). e.g. a DID
    /// If the audience is already present in the JWT-Artefacts, no value has to be provided. If a value is provided anyway, it will override the value exising in the JWT</param>
    /// <param name="alg">The algorithm used for signing (if the JWT will be signed) e.g. ES256K (for secp256k1 in PRISM) or EdDSA (for Ed25519VerificationKey2018 in Indy) </param>
    /// <param name="kid">kid MAY be used if there are multiple keys associated with the issuer of the JWT. e.g. did:prism:something#key-1</param>
    /// <param name="options">Set of options which can be left as default</param>
    /// <param name="additionalClaims">if the JWT should also contain additional data. Be sure not to overlap the keys with the pre-defined keys for a JWT</param>
    /// <returns></returns>
    public static Result<JwtModel> Build(List<VerifiablePresentation> presentations, Uri? audience, string? alg, string? kid, JwtBuilderOptions options, Dictionary<string, object>? additionalClaims = null)
    {
        if (options.BuildJwtWithoutSignature)
        {
            if (presentations.Select(p => p.Proofs).Any(p => p == null))
            {
                return Result.Fail("Cannot build a JWT without a signature if the presenation has no proof");
            }
        }
        else
        {
            // If we are creating a signature (JWS) then we can omit the proof from the credential
            if (options.OmitProofFromCredentialOrPresentation)
            {
                presentations = presentations.Select(p => p with { Proofs = null }).ToList();
            }
        }

        var expirationDate = presentations.SelectMany(p => p.VerifiableCredentials?.Select(q => q.ExpirationDate) ?? Array.Empty<DateTime?>()).Where(p => p != null).Min();
        var notValidBefore = presentations.SelectMany(p => p.VerifiableCredentials?.Select(q => q.IssuanceDate) ?? Array.Empty<DateTime?>()).Max();
        var validUntil = presentations.SelectMany(p => p.VerifiableCredentials?.Select(q => q.ValidUntil) ?? Array.Empty<DateTime?>()).Where(p => p != null).Min();
        var validFrom = presentations.SelectMany(p => p.VerifiableCredentials?.Select(q => q.ValidFrom) ?? Array.Empty<DateTime?>()).Where(p => p != null).Max();

        var startDate = notValidBefore ?? validFrom;
        var endDate = expirationDate ?? validUntil;

        if (startDate is not null && startDate.Value.Kind == DateTimeKind.Unspecified)
        {
            startDate = DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc);
        }

        if (endDate is not null && endDate.Value.Kind == DateTimeKind.Unspecified)
        {
            endDate = DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc);
        }

        var holders = presentations.Select(p => p.Holder).Distinct().ToList();
        if (holders.Count() > 1)
        {
            return Result.Fail("Cannot build a JWT with multiple holderes. All holders of the presentation must be the same.");
        }

        var jwtId = presentations.Select(p => p.Id).ToHashSet();
        if (jwtId.Count() > 1)
        {
            return Result.Fail("Cannot build a JWT with multiple ids. All ids of the presentation must be the same.");
        }

        if (audience is null)
        {
            var audiencesFromJwt = presentations.Select(p => p.JwtParsingArtefact?.Audience).Distinct().Where(p => p != null).ToList();
            if (audiencesFromJwt.Count == 1)
            {
                // A Audience was already present in the JWT
                audience = audiencesFromJwt.Single();
            }
            else
            {
                return Result.Fail("A audience value must be provided when creating a VerifiablePresentation as JWT. Either provide the audience as parameter or as a claim in the JWT-Artefacts. Providing an audiance as an parameter will override the value in the JWT-Artefacts");
            }
        }

        var result = BuildInteral(null, presentations, options, startDate, endDate, holders!.Single(), jwtId.FirstOrDefault(), null, audience, alg, kid, options.PrivateKeyForJwtSigningOfEmbeddedCredentialsInsidePresentation, additionalClaims);
        if (result.IsFailed)
        {
            return result.ToResult();
        }

        return result.Value;
    }

    private static Result<JwtModel> BuildInteral(
        List<VerifiableCredential>? credentials,
        List<VerifiablePresentation>? presentations,
        JwtBuilderOptions options,
        DateTime? startDate,
        DateTime? endDate,
        Uri issuer,
        Uri? jwtId,
        Uri? subject,
        Uri? audience,
        string? headerAlg,
        string? kid,
        string? privateKeyForJwtSigning,
        Dictionary<string, object>? additionalClaims = null)
    {
        // alg MUST be set for digital signatures. If only the proof property is needed for the chosen signature
        // method (that is, if there is no choice of algorithm within that method), the alg header MUST be set to none.
        string alg = "none";
        if (options.BuildJwtWithoutSignature)
        {
            alg = "none";
        }
        else if (headerAlg is not null)
        {
            alg = headerAlg;
        }
        else
        {
            return Result.Fail("The alg header must be set to none or a valid signature algorithm if a signature is used");
        }

        var jwtHeader = new JwtCredentialHeaders()
        {
            Alg = alg,
            // kid MAY be used if there are multiple keys associated with the issuer of the JWT. The key discovery is out of the scope of this specification.
            Kid = kid,
            Typ = "JWT"
        };


        if (additionalClaims is not null)
        {
            if (additionalClaims.ContainsKey("alg") || additionalClaims.ContainsKey("kid") || additionalClaims.ContainsKey("typ"))
            {
                return Result.Fail("Cannot build a JWT with additional header data that contains alg, kid or typ");
            }

            if (additionalClaims.ContainsKey("exp") || additionalClaims.ContainsKey("iss") || additionalClaims.ContainsKey("nbf"))
            {
                return Result.Fail("Cannot build a JWT with additional header data that contains alg, kid or typ");
            }
        }

        if (credentials is not null)
        {
            foreach (var credential in credentials.Where(p => p.JwtParsingArtefact?.RemoveSubjectIdFromCredentialAndReplaceWithClaim == true))
            {
                for (int i = 0; i < credential.CredentialSubjects.Count; i++)
                {
                    credential.CredentialSubjects[i] = credential.CredentialSubjects[i] with { Id = null };
                }
            }

            credentials = credentials.Select(credential =>
                    credential.JwtParsingArtefact?.RemoveExpirationDateFromCredentialAndReplaceWithClaim == true
                        ? credential with { ExpirationDate = null }
                        : credential)
                .ToList();

            credentials = credentials.Select(credential =>
                    credential.JwtParsingArtefact?.RemoveIssuanceDateFromCredentialAndReplaceWithClaim == true
                        ? credential with { IssuanceDate = null }
                        : credential)
                .ToList();

            credentials = credentials.Select(credential =>
                    credential.JwtParsingArtefact?.RemoveIssuerOrHolderFromCredentialAndReplaceWithClaim == true
                        ? credential with { CredentialIssuer = null }
                        : credential)
                .ToList();

            credentials = credentials.Select(credential =>
                    credential.JwtParsingArtefact?.RemoveIdFromCredentialOrPresentationAndReplaceWithClaim == true
                        ? credential with { Id = null }
                        : credential)
                .ToList();
        }

        if (presentations is not null)
        {
            presentations = presentations.Select(presentation =>
                    presentation.JwtParsingArtefact?.RemoveIssuerOrHolderFromCredentialAndReplaceWithClaim == true
                        ? presentation with { Holder = null }
                        : presentation)
                .ToList();

            presentations = presentations.Select(presentation =>
                    presentation.JwtParsingArtefact?.RemoveIdFromCredentialOrPresentationAndReplaceWithClaim == true
                        ? presentation with { Id = null }
                        : presentation)
                .ToList();
        }

        var jwtPayload = new JwtCredentialClaims()
        {
            VerifiablePresentations = presentations,
            VerifiableCredentials = credentials,
            // For backward compatibility with JWT processors, the following registered JWT claim names MUST be used,
            // instead of or in addition to, their respective standard verifiable credential counterparts:
            // exp MUST represent the expirationDate property, encoded as a UNIX timestamp (NumericDate).
            // It is a bit unclear, when we have multiple credentials, which expirationDate should be used. But we
            // take the earlierst one.
            Exp = endDate is not null ? new DateTimeOffset(endDate.Value).ToUnixTimeSeconds() : null,
            // nbf MUST represent issuanceDate, encoded as a UNIX timestamp (NumericDate).
            Nbf = startDate is not null ? new DateTimeOffset(startDate.Value).ToUnixTimeSeconds() : null,
            // iss MUST represent the issuer property of a verifiable credential or the holder property of a verifiable presentation.
            Iss = issuer.OriginalString,
            // jti MUST represent the id property of the verifiable credential or verifiable presentation.
            Jti = jwtId?.OriginalString,
            // sub MUST represent the id property contained in the credentialSubject
            Sub = subject?.OriginalString,
            // MUST represent (i.e., identify) the intended audience of the verifiable presentation (i.e., the verifier intended by
            // the presenting holder to receive and verify the verifiable presentation).
            // Aud <- Not present in a JWT for a credential
            Aud = audience?.OriginalString,
            AdditionalClaims = additionalClaims,
            // The algorithm used for signing (if the JWT will be signed) e.g. ES256K (for secp256k1 in PRISM) or EdDSA (for Ed25519VerificationKey2018 in Indy)
            Algorithm = alg,
            // Signing will only happen if the privateKey is provided and the alg is not "none" and the BuildJwtWithoutSignature is false
            PrivateKey = privateKeyForJwtSigning,
            BuildJwtWithoutSignature = options.BuildJwtWithoutSignature
        };

        var jsonSerializerOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JwtCredentialClaimsConverter() }
        };

        var headers = JsonSerializer.Serialize(jwtHeader, jsonSerializerOptions);
        var payload = JsonSerializer.Serialize(jwtPayload, jsonSerializerOptions);

        return Result.Ok(new JwtModel(headers, payload));
    }


    public static JwtModel SignJwt(JwtModel jwtModel, string alg, string privateKey)
    {
        throw new NotImplementedException();
        // return Jose.JWT.Encode(jwtModel.Payload, privateKey, Jose.JwsAlgorithm.ES256);
    }

    public static JwtModel SignJwt(JwtModel jwtModel, string signature)
    {
        jwtModel.Signature = signature;
        return jwtModel;
    }

    public static string EncodeAsJwt(JwtModel jwtModel)
    {
        var encodedHeader = Base64Url.Encode(Encoding.UTF8.GetBytes(jwtModel.HeadersAsJson!));
        var encodedPayload = Base64Url.Encode(Encoding.UTF8.GetBytes(jwtModel.PayloadAsJson!));
        if (jwtModel.Signature is not null)
        {
            return $"{encodedHeader}.{encodedPayload}.{jwtModel.Signature}";
        }

        return $"{encodedHeader}.{encodedPayload}";
    }
}