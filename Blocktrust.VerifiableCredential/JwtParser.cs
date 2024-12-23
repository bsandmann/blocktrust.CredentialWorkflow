// ReSharper disable NullableWarningSuppressionIsUsed

namespace Blocktrust.VerifiableCredential;

using System.Text.Json;
using Common.JwtModels;
using FluentResults;
using VC;
using Base64Url = Common.Base64Url;

public static class 
    JwtParser
{
    private static readonly List<string> AllowedJwtAlgorithms = new List<string>()
    {
        "HS256",
        "HS384",
        "HS512",
        "RS256",
        "RS384",
        "RS512",
        "ES256",
        "ES384",
        "ES512",
        "ES256K",
        "PS256",
        "PS384",
        "PS512",
        "EDDSA",
        "NONE"
    };

    private static readonly List<string> SupportedJwtAlgorithms = new List<string>()
    {
        "ES256K",
        "EDDSA",
        "NONE"
    };

    public static Result<ParsedJwt> Parse(string base64String)
    {
        var jwt = ExtractHeaderAndPayloadFromBase64(base64String);
        if (jwt.IsFailed)
        {
            return jwt.ToResult();
        }

        return Parse(jwt.Value);
    }

    public static Result<JwtModel> ExtractHeaderAndPayloadFromBase64(string content)
    {
        var parts = content.Trim().Split('.');
        if (parts.Length == 2 || parts.Length == 3)
        {
            var headersAsBytes = Base64Url.Decode(parts[0]);
            var headersAsString = System.Text.Encoding.UTF8.GetString(headersAsBytes);
            var headers = JsonSerializer.Deserialize<Dictionary<string, object>>(headersAsString);
            if (headers is null || headers.Count == 0)
            {
                return Result.Fail("Invalid JWT. Could not extract headers.");
            }

            var payloadAsBytes = Base64Url.Decode(parts[1]);
            var payloadAsString = System.Text.Encoding.UTF8.GetString(payloadAsBytes);
            var payload = JsonSerializer.Deserialize<Dictionary<string, object>>(payloadAsString);
            if (payload is null || payload.Count == 0)
            {
                return Result.Fail("Invalid JWT. Could not extract payload.");
            }

            if (parts.Length == 3)
            {
                // JWT with signature
                if (string.IsNullOrEmpty(parts[2]))
                {
                    return Result.Fail("Invalid JWT. Could not extract signature.");
                }

                return Result.Ok(new JwtModel(headers.ToDictionary(), payload, parts[2], headersAsString, payloadAsString));
            }
            else if (parts.Length == 2)
            {
                // JWT without signature    
                return Result.Ok(new JwtModel(headers.ToDictionary(), payload, null, headersAsString, payloadAsString));
            }
        }


        return Result.Fail("Invalid JWT in Base64 format.");
    }

    private static Result<ParsedJwt> Parse(JwtModel jwt)
    {
        if (jwt.HeadersAsJson is null)
        {
            return Result.Fail("This parser requires the JWT headers to be in JSON format.");
        }

        var headers = JsonDocument.Parse(jwt.HeadersAsJson);
        var typ = headers.RootElement.TryGetProperty("typ", out var typElement) ? typElement.GetString() : null;
        if (typ is null || !typ.Equals("JWT", StringComparison.InvariantCultureIgnoreCase))
        {
            // The type value is actually optional. See https://www.rfc-editor.org/rfc/rfc7519#section-5.1
            // return Result.Fail("Invalid JWT. The typ header must be 'JWT'.");
        }

        var alg = headers.RootElement.TryGetProperty("alg", out var algElement) ? algElement.GetString() : null;
        if (string.IsNullOrEmpty(alg))
        {
            return Result.Fail("Invalid JWT. The alg header must be present.");
        }

        if (!AllowedJwtAlgorithms.Contains(alg.ToUpperInvariant()))
        {
            return Result.Fail($"Invalid JWT. The alg header must be one of the following: {string.Join(", ", AllowedJwtAlgorithms)}");
        }

        //Execute this code only in a production environment
#if RELEASE
        if (!SupportedJwtAlgorithms.Contains(alg.ToUpperInvariant()))
        {
            return Result.Fail($"Invalid JWT. Only these alg-headers are currently supported: {string.Join(", ", SupportedJwtAlgorithms)}");
        }
#endif
        var kid = headers.RootElement.TryGetProperty("kid", out var kidElement) ? kidElement.GetString() : null;

        var exp = jwt.Payload.TryGetValue("exp", out var expElement) ? expElement.ToString() : null;
        var iss = jwt.Payload.TryGetValue("iss", out var issElement) ? issElement.ToString() : null;
        var nbf = jwt.Payload.TryGetValue("nbf", out var nbfElement) ? nbfElement.ToString() : null;
        var sub = jwt.Payload.TryGetValue("sub", out var subElement) ? subElement.ToString() : null;
        var jti = jwt.Payload.TryGetValue("jti", out var jtiElement) ? jtiElement.ToString() : null;
        var aud = jwt.Payload.TryGetValue("aud", out var audElement) ? audElement.ToString() : null;

        var hasVc = jwt.Payload.TryGetValue("vc", out var vcElement);
        var hasVp = jwt.Payload.TryGetValue("vp", out var vpElement);
        var vcList = new List<VerifiableCredential>();
        var vpList = new List<VerifiablePresentation>();
        if (hasVc && vcElement is JsonElement vcJsonElement)
        {
            if (vcJsonElement.ValueKind == JsonValueKind.Object)
            {
                var vc = JsonSerializer.Deserialize<VerifiableCredential>(vcJsonElement.GetRawText());
                if (vc is null)
                {
                    return Result.Fail("Invalid JWT. Could not extract Verifiable Credential.");
                }

                vc.DataModelType = DataModelTypeEvaluator.Evaluate(vc);

                // Initially setting the JWT parsing artefacts to something
                vc.JwtParsingArtefact = new JwtParsingArtefact();

                vcList.Add(vc);
            }
            else if (vcJsonElement.ValueKind == JsonValueKind.Array)
            {
                var multipleVc = JsonSerializer.Deserialize<List<VerifiableCredential>>(vcJsonElement.GetRawText());
                if (multipleVc is null || multipleVc.Count == 0)
                {
                    return Result.Fail("Invalid JWT. Could not extract Verifiable Credential.");
                }

                multipleVc.ForEach(p => p.DataModelType = DataModelTypeEvaluator.Evaluate(p));
                multipleVc.ForEach(p => p.JwtParsingArtefact = new JwtParsingArtefact());

                vcList.AddRange(multipleVc);
            }
            else
            {
                return Result.Fail("Invalid JWT. Could not extract Verifiable Credential.");
            }
        }
        else if (hasVp && vpElement is JsonElement vpJsonElement)
        {
            if (vpJsonElement.ValueKind == JsonValueKind.Object)
            {
                var vp = JsonSerializer.Deserialize<VerifiablePresentation>(vpJsonElement.GetRawText());
                if (vp is null)
                {
                    return Result.Fail("Invalid JWT. Could not extract Verifiable Presentation.");
                }

                vp.DataModelType = DataModelTypeEvaluator.Evaluate(vp);

                // Initially setting the JWT parsing artefacts to something
                vp.JwtParsingArtefact = new JwtParsingArtefact()
                {
                    // only VPs can have an audience
                    Audience = aud is not null ? new Uri(aud) : null
                };


                vpList.Add(vp);
            }
            else if (vpJsonElement.ValueKind == JsonValueKind.Array)
            {
                var multipleVp = JsonSerializer.Deserialize<List<VerifiablePresentation>>(vpJsonElement.GetRawText());
                if (multipleVp is null || multipleVp.Count == 0)
                {
                    return Result.Fail("Invalid JWT. Could not extract Verifiable Presentation.");
                }

                multipleVp.ForEach(p => p.DataModelType = DataModelTypeEvaluator.Evaluate(p));
                multipleVp.ForEach(p => p.JwtParsingArtefact = new JwtParsingArtefact()
                {
                    // only VPs can have an audience
                    Audience = aud is not null ? new Uri(aud) : null
                });

                vpList.AddRange(multipleVp);
            }
            else
            {
                return Result.Fail("Invalid JWT. Could not extract Verifiable Presentation.");
            }
        }

        // Post-validation if the JWT claims match the Verifiable Credential
        if (exp is not null)
        {
            // If exp is present, the UNIX timestamp MUST be converted to an [XMLSCHEMA11-2] date-time, and MUST be used
            // to set the value of the expirationDate property of credentialSubject of the new JSON object.
            for (var i = 0; i < vcList.Count; i++)
            {
                var expirationDateOfCredential = vcList[i].ExpirationDate;
                if (expirationDateOfCredential is null && vcList[i].DataModelType == EDataModelType.DataModel11)
                {
                    // The expirationDate property was likely removed from the Credential and just placed in the header as "exp" claim
                    vcList[i] = vcList[i] with { ExpirationDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp)).DateTime };
                    // we take not of this modification so that we can potentially rebuild the idential JWT 
                    vcList[i] = vcList[i] with { JwtParsingArtefact = vcList[i].JwtParsingArtefact! with { RemoveExpirationDateFromCredentialAndReplaceWithClaim = true } };
                }
                else if (expirationDateOfCredential is null && vcList[i].DataModelType == EDataModelType.DataModel2)
                {
                    throw new NotImplementedException();
                }
                else if (expirationDateOfCredential is not null && vcList[i].DataModelType == EDataModelType.DataModel11)
                {
                    if (expirationDateOfCredential.Value != DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp)).DateTime)
                    {
                        vcList[i].JwtParsingArtefact!.JwtParsingWarnings!.Add("The expiration date of the Verifiable Credential does not match the expiration date of the JWT. The expiration date of the Verifiable Credential will be overritten with the 'exp' value of the JWT.");
                        vcList[i] = vcList[i] with { ExpirationDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp)).DateTime };
                    }
                }
                else if (expirationDateOfCredential is not null && vcList[i].DataModelType == EDataModelType.DataModel2)
                {
                    throw new NotImplementedException();
                }
            }
        }

        if (nbf is not null)
        {
            // If nbf is present, the UNIX timestamp MUST be converted to an [XMLSCHEMA11-2] date-time, and MUST be 
            // used to set the value of the issuanceDate property of the new JSON object.
            for (var i = 0; i < vcList.Count; i++)
            {
                var issuanceDateOfCredential = vcList[i].IssuanceDate;
                if (issuanceDateOfCredential is null && vcList[i].DataModelType == EDataModelType.DataModel11)
                {
                    // The issuanceDate property was likely removed from the Credential and just placed in the header as "nbf" claim
                    vcList[i] = vcList[i] with { IssuanceDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(nbf)).DateTime };
                    // we take not of this modification so that we can potentially rebuild the idential JWT 
                    vcList[i] = vcList[i] with { JwtParsingArtefact = vcList[i].JwtParsingArtefact! with { RemoveIssuanceDateFromCredentialAndReplaceWithClaim = true } };
                }
                else if (issuanceDateOfCredential is null && vcList[i].DataModelType == EDataModelType.DataModel2)
                {
                    throw new NotImplementedException();
                }
                else if (issuanceDateOfCredential is not null && vcList[i].DataModelType == EDataModelType.DataModel11)
                {
                    if (issuanceDateOfCredential.Value != DateTimeOffset.FromUnixTimeSeconds(long.Parse(nbf)).DateTime)
                    {
                        vcList[i].JwtParsingArtefact!.JwtParsingWarnings!.Add("The issuanceDate of the Verifiable Credential does not match the not-before-date (nbf) of the JWT. The issuance date of the Verifiable Credential will be overritten with the 'nbf' value of the JWT.");
                        vcList[i] = vcList[i] with { ExpirationDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(nbf)).DateTime };
                    }
                }
                else if (issuanceDateOfCredential is not null && vcList[i].DataModelType == EDataModelType.DataModel2)
                {
                    throw new NotImplementedException();
                }
            }
        }

        if (iss is not null)
        {
            // If iss is present, the value MUST be used to set the issuer property of the new credential JSON object
            // or the holder property of the new presentation JSON object.
            for (var i = 0; i < vcList.Count; i++)
            {
                var issuerOfCredential = vcList[i].CredentialIssuer;
                if (issuerOfCredential is null)
                {
                    // usually the issuer of a credemtial is a required property, but in this case we can add it from the JWT
                    // The issuer property was likely removed from the Credential and just placed in the header as "iss" claim
                    vcList[i] = vcList[i] with { CredentialIssuer = new CredentialIssuer(new Uri(iss)) };
                    // we take not of this modification so that we can potentially rebuild the idential JWT 
                    vcList[i] = vcList[i] with { JwtParsingArtefact = vcList[i].JwtParsingArtefact! with { RemoveIssuerOrHolderFromCredentialAndReplaceWithClaim = true } };
                }
                else
                {
                    var issuerId = issuerOfCredential.IssuerId;
                    var issUri = new Uri(iss);
                    if (!issUri.AbsoluteUri.Equals(issuerId.AbsoluteUri, StringComparison.InvariantCultureIgnoreCase) || !issUri.OriginalString.Equals(issuerId.OriginalString, StringComparison.InvariantCultureIgnoreCase))
                    {
                        vcList[i].JwtParsingArtefact!.JwtParsingWarnings!.Add("The iss-claims (Issuer) does not match the issuer propery of the verifiable credential. The issuer property of the verifiable credential will be overritten with the 'iss' value of the JWT.");
                        if (issuerOfCredential.IssuerDescription is not null || issuerOfCredential.IssuerName is not null)
                        {
                            vcList[i].JwtParsingArtefact!.JwtParsingWarnings!.Add("By overwritting the issuer property of the verifiable credential, the issuer-Name and issuer-Description values will be removed");
                        }
                        else if (issuerOfCredential.IssuerDescriptionLanguages is not null || issuerOfCredential.IssuerNameLanguages is not null)
                        {
                            vcList[i].JwtParsingArtefact!.JwtParsingWarnings!.Add("By overwritting the issuer property of the verifiable credential, the issuer-Name with for different languages and issuer-Description for different languages properties will be removed");
                        }

                        vcList[i] = vcList[i] with { CredentialIssuer = new CredentialIssuer(issUri) };
                    }
                }
            }

            for (var i = 0; i < vpList.Count; i++)
            {
                var issuerOfPresentation = vpList[i].Holder;
                if (issuerOfPresentation is null)
                {
                    // The holder property was likely removed from the Presentaiton and just placed in the header as "iss" claim
                    vpList[i] = vpList[i] with { Holder = new Uri(iss) };
                    // we take not of this modification so that we can potentially rebuild the idential JWT 
                    vpList[i] = vpList[i] with { JwtParsingArtefact = vpList[i].JwtParsingArtefact! with { RemoveIssuerOrHolderFromCredentialAndReplaceWithClaim = true } };
                }
                else
                {
                    var issUri = new Uri(iss);
                    if (!issUri.AbsoluteUri.Equals(issuerOfPresentation.AbsoluteUri, StringComparison.InvariantCultureIgnoreCase) || !issUri.OriginalString.Equals(issuerOfPresentation.OriginalString, StringComparison.InvariantCultureIgnoreCase))
                    {
                        vpList[i].JwtParsingArtefact!.JwtParsingWarnings!.Add("The iss-claims (Issuer) does not match the holder propery of the verifiable presentation. The holder property of the verifiable presentation will be overritten with the 'iss' value of the JWT.");
                        vpList[i] = vpList[i] with { Holder = issUri };
                    }
                }
            }
        }

        if (sub is not null)
        {
            // If sub is present, the value MUST be used to set the value of the id property of credentialSubject of the new credential JSON object. 
            for (int i = 0; i < vcList.Count; i++)
            {
                var subUri = new Uri(sub);
                var credentialSubject = vcList[i].CredentialSubjects.ToList();
                if (credentialSubject.Count == 1)
                {
                    var credentialSubjectId = credentialSubject[0].Id;
                    if (credentialSubjectId is not null)
                    {
                        if (!subUri.AbsoluteUri.Equals(credentialSubjectId.AbsoluteUri, StringComparison.InvariantCultureIgnoreCase))
                        {
                            vcList[i].JwtParsingArtefact!.JwtParsingWarnings!.Add(
                                "The sub-claims (Subject) of the JWT does not match the id property of the credentialSubject of the verifiable credential. The id property of the credentialSubject of the verifiable credential will be overritten with the 'sub' value of the JWT.");
                            vcList[i] = vcList[i] with { CredentialSubjects = new List<CredentialSubject>() { vcList[i].CredentialSubjects[0] with { Id = subUri } } };
                        }
                    }
                    else
                    {
                        // The sub property was likely removed from the credentialSubject and just placed in the header as "sub" claim
                        vcList[i] = vcList[i] with { CredentialSubjects = new List<CredentialSubject>() { vcList[i].CredentialSubjects[0] with { Id = subUri } } };
                        // we take not of this modification so that we can potentially rebuild the idential JWT 
                        vcList[i] = vcList[i] with { JwtParsingArtefact = vcList[i].JwtParsingArtefact! with { RemoveSubjectIdFromCredentialAndReplaceWithClaim = true } };
                    }
                }
                else if (credentialSubject.Count > 1)
                {
                    vcList[i].JwtParsingArtefact!.JwtParsingWarnings!.Add(
                        "The Subject of the VerifiableCredential contains multiple credentialSubjects. The id property of the credentialSubject of the verifiable credential will be overritten with the 'sub' value of the JWT and therefor remove all other credentialSubjects and additional subject properties.");
                    vcList[i] = vcList[i] with { CredentialSubjects = new List<CredentialSubject>() { vcList[i].CredentialSubjects[i] with { Id = subUri } } };
                }
            }

            // No need to do anything for presentations
        }

        if (jti is not null)
        {
            // if jti is present, the value MUST be used to set the value of the id property of the new JSON object.
            if (vcList.Count == 1)
            {
                if (vcList[0].Id is null)
                {
                    // The id property was likely removed from the Credential and just placed in the header as "jti" claim
                    var jtiIsUri = Uri.TryCreate(jti, UriKind.RelativeOrAbsolute, out Uri? jtiUri);
                    if (jtiIsUri)
                    {
                        vcList[0] = vcList[0] with { Id = jtiUri };
                        // we take not of this modification so that we can potentially rebuild the idential JWT 
                        vcList[0] = vcList[0] with { JwtParsingArtefact = vcList[0].JwtParsingArtefact! with { RemoveIdFromCredentialOrPresentationAndReplaceWithClaim = true } };
                    }
                }
                else if (vcList[0].Id is not null && !vcList[0].Id!.OriginalString.Equals(jti, StringComparison.InvariantCultureIgnoreCase) && !vcList[0].Id!.AbsoluteUri.Equals(jti, StringComparison.InvariantCultureIgnoreCase))
                {
                    vcList[0].JwtParsingArtefact!.JwtParsingWarnings!.Add("The jti-claims (JWT ID) does not match the id property of the verifiable credential. The id property of the verifiable credential will be overritten with the 'jti' value of the JWT.");
                    var jtiIsUri = Uri.TryCreate(jti, UriKind.RelativeOrAbsolute, out Uri? jtiUri);
                    if (jtiIsUri)
                    {
                        vcList[0] = vcList[0] with { Id = jtiUri };
                    }
                }
            }
            else if (vcList.Count > 1)
            {
                var idWasChanged = false;
                for (int i = 0; i < vcList.Count; i++)
                {
                    if (vcList[i].Id is null)
                    {
                        if (idWasChanged)
                        {
                            // The id property was likely removed from the Credential and just placed in the header as "jti" claim
                            vcList[i].JwtParsingArtefact!.JwtParsingWarnings!.Add("Multiple Id properties of Verifiable Credentials are set to the same id based on the jti-claims (JWT ID). This is not recommended!");
                            // we take not of this modification so that we can potentially rebuild the idential JWT 
                            vcList[i] = vcList[i] with { JwtParsingArtefact = vcList[i].JwtParsingArtefact! with { RemoveIdFromCredentialOrPresentationAndReplaceWithClaim = true } };
                        }

                        var jtiIsUri = Uri.TryCreate(jti, UriKind.RelativeOrAbsolute, out Uri? jtiUri);
                        if (jtiIsUri)
                        {
                            vcList[i] = vcList[i] with { Id = jtiUri };
                            idWasChanged = true;
                        }
                    }
                    else if (vcList[i].Id is not null && !vcList[i].Id!.OriginalString.Equals(jti, StringComparison.InvariantCultureIgnoreCase) && !vcList[i].Id!.AbsoluteUri.Equals(jti, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (idWasChanged)
                        {
                            vcList[i].JwtParsingArtefact!.JwtParsingWarnings!.Add("Multiple Id properties of Verifiable Credentials are set to the same id based on the jti-claims (JWT ID). This is not recommended!");
                        }

                        vcList[i].JwtParsingArtefact!.JwtParsingWarnings!.Add("The jti-claims (JWT ID) does not match the id property of the verifiable credential. The id property of the verifiable credential will be overritten with the 'jti' value of the JWT.");
                        var jtiIsUri = Uri.TryCreate(jti, UriKind.RelativeOrAbsolute, out Uri? jtiUri);
                        if (jtiIsUri)
                        {
                            vcList[i] = vcList[i] with { Id = jtiUri };
                            idWasChanged = true;
                        }
                    }
                }
            }

            if (vpList.Count == 1)
            {
                if (vpList[0].Id is null)
                {
                    // The id property was likely removed from the Presentation and just placed in the header as "jti" claim
                    var jtiIsUri = Uri.TryCreate(jti, UriKind.RelativeOrAbsolute, out Uri? jtiUri);
                    if (jtiIsUri)
                    {
                        vpList[0] = vpList[0] with { Id = jtiUri };
                    }

                    // we take not of this modification so that we can potentially rebuild the idential JWT 
                    vpList[0] = vpList[0] with { JwtParsingArtefact = vpList[0].JwtParsingArtefact! with { RemoveIdFromCredentialOrPresentationAndReplaceWithClaim = true } };
                }
                else if (vpList[0].Id is not null && !vpList[0].Id!.OriginalString.Equals(jti, StringComparison.InvariantCultureIgnoreCase) && !vpList[0].Id!.AbsoluteUri.Equals(jti, StringComparison.InvariantCultureIgnoreCase))
                {
                    vpList[0].JwtParsingArtefact!.JwtParsingWarnings!.Add("The jti-claims (JWT ID) does not match the id property of the verifiable presentation. The id property of the verifiable presentation will be overritten with the 'jti' value of the JWT.");
                    var jtiIsUri = Uri.TryCreate(jti, UriKind.RelativeOrAbsolute, out Uri? jtiUri);
                    if (jtiIsUri)
                    {
                        vpList[0] = vpList[0] with { Id = jtiUri };
                    }
                }
            }
            else if (vpList.Count > 1)
            {
                var idWasChanged = false;
                for (int i = 0; i < vpList.Count; i++)
                {
                    if (vpList[i].Id is null)
                    {
                        if (idWasChanged)
                        {
                            vpList[i].JwtParsingArtefact!.JwtParsingWarnings!.Add("Multiple Id properties of Verifiable Presentations are set to the same id based on the jti-claims (JWT ID). This is not recommended!");
                        }

                        var jtiIsUri = Uri.TryCreate(jti, UriKind.RelativeOrAbsolute, out Uri? jtiUri);
                        if (jtiIsUri)
                        {
                            vpList[i] = vpList[i] with { Id = jtiUri };
                            idWasChanged = true;
                        }
                    }
                    else if (vpList[i].Id is not null && !vpList[i].Id!.OriginalString.Equals(jti, StringComparison.InvariantCultureIgnoreCase) && !vpList[i].Id!.AbsoluteUri.Equals(jti, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (idWasChanged)
                        {
                            vpList[i].JwtParsingArtefact!.JwtParsingWarnings!.Add("Multiple Id properties of Verifiable Presentations are set to the same id based on the jti-claims (JWT ID). This is not recommended!");
                        }

                        vpList[i].JwtParsingArtefact!.JwtParsingWarnings!.Add("The jti-claims (JWT ID) does not match the id property of the verifiable presentation. The id property of the verifiable presentation will be overritten with the 'jti' value of the JWT.");
                        var jtiIsUri = Uri.TryCreate(jti, UriKind.RelativeOrAbsolute, out Uri? jtiUri);
                        if (jtiIsUri)
                        {
                            vpList[i] = vpList[i] with { Id = jtiUri };
                            idWasChanged = true;
                        }
                    }
                }
            }
        }

        var reservedClaimsList = new List<string>() { "sub", "iss", "aud", "exp", "nbf", "jti", "vc", "vp" };
        var additionalPayloadData = jwt.Payload.Where(p => !reservedClaimsList.Contains(p.Key, StringComparer.InvariantCultureIgnoreCase)).ToDictionary();

        if (vcList.Any())
        {
            if (additionalPayloadData.Any())
            {
                vcList.ForEach(p => p.JwtParsingArtefact!.JwtAdditionalClaims = additionalPayloadData);
            }

            if (!string.IsNullOrEmpty(kid))
            {
                vcList.ForEach(p => p.JwtParsingArtefact!.JwtVerificationKeyId = kid);
            }

            if (jwt.Signature is not null)
            {
                vcList.ForEach(p => p.JwtParsingArtefact!.JwtSignature = jwt.Signature);
            }

            vcList.ForEach(p => p.JwtParsingArtefact!.JwtAlg = alg);

            return Result.Ok(new ParsedJwt(vcList));
        }
        else if (vpList.Any())
        {
            if (additionalPayloadData.Any())
            {
                vpList.ForEach(p => p.JwtParsingArtefact!.JwtAdditionalClaims = additionalPayloadData);
            }

            if (!string.IsNullOrEmpty(kid))
            {
                vpList.ForEach(p => p.JwtParsingArtefact!.JwtVerificationKeyId = kid);
            }

            if (jwt.Signature is not null)
            {
                vpList.ForEach(p => p.JwtParsingArtefact!.JwtSignature = jwt.Signature);
            }

            vpList.ForEach(p => p.JwtParsingArtefact!.JwtAlg = alg);

            return Result.Ok(new ParsedJwt(vpList));
        }
        else
        {
            return Result.Fail("Invalid JWT. Could not extract Verifiable Credential or Verifiable Presentation.");
        }
    }
}