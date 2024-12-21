namespace Blocktrust.VerifiableCredential.Common.JwtModels;

using System.Text.Json.Serialization;

public record JwtParsingArtefact
{
    public JwtParsingArtefact()
    {
        JwtParsingWarnings = new List<string>();
        JwtAdditionalClaims = new Dictionary<string, object>();
    }

    /// <summary>
    /// Artefact of the JWT parsing process. If a JWT is parsed, this property contains the key id of the
    /// keyId that was used to verify the signature of the JWT. This value is optional, and doesn't need
    /// to be included
    /// </summary>
    [JsonIgnore]
    public string? JwtVerificationKeyId { get; set; }

    /// <summary>
    /// Artefact of the JWT parsing process. If a JWT is parsed, this property contains the warnings that
    /// might occur during the parsing process. This value is optional, and doesn't need to be included
    /// </summary>
    [JsonIgnore]
    public List<string>? JwtParsingWarnings { get; init; }

    /// <summary>
    /// Potential audience of the JWT when it is a presentation
    /// https://www.w3.org/TR/vc-data-model/#what-is-a-verifiable-credential
    /// </summary>
    [JsonIgnore]
    public Uri? Audience { get; init; }

    /// <summary>
    /// Matches the <see cref="Audience"/>
    /// Optional link to reference an exisiting DID Dataset
    /// Not part of any Equality comparision of Json Serialization
    /// </summary>
    [JsonIgnore]
    public Guid? DidDatasetVerifiablePresentationAudienceId { get; set; }
    
    /// <summary>
    /// Matches the <see cref="Audience"/>
    /// in case the Id is a valid URI pointing to a website
    /// to represent the Presentation Audience
    /// </summary>
    [JsonIgnore]
    public int? IdentityUriVerifiablePresentationAudienceId { get; set; }

    /// <summary>
    /// Artefact of the JWT parsing process. If a JWT is parsed, this property contains the additional claims
    /// e.g. "nonce", "iat", etc from the JWT. This value is optional, and doesn't need to be included
    /// </summary>
    [JsonIgnore]
    public IDictionary<string, object>? JwtAdditionalClaims { get; set; }


    /// <summary>
    /// Artefact of the JWT parsing process. If a JWT is parsed, this property contains the algorithm
    /// in the header of the JWT (e.g. "ES256K"). This value is optional, and doesn't need to be included
    /// </summary>
    [JsonIgnore]
    public string? JwtAlg { get; set; }

    /// <summary>
    /// Artefact of the JWT parsing process. If a JWT is parsed, this property contains the signature
    /// used to sign the JWT. This value is optional, and doesn't need to be included
    /// </summary>
    [JsonIgnore]
    public string? JwtSignature { get; set; }

    /// <summary>
    /// Property to pass down a privateKey to the Jwt-Builder for embedded Credentials inside a VerifiablePresentations
    /// in case they should be written as JWTs
    /// </summary>
    [JsonIgnore]
    public string? PrivateKeyForSigningJwtCredentialsInsidePresentations { get; init; }

    /// <summary>
    /// JWTs allows to omit the Id property of the CredentialSubject and place it in the header as a "sub" claim instead.
    /// Settings this flag to true will remove the Id property from the CredentialSubject. Otherwise it will be duplicated in the header.
    /// </summary>
    [JsonIgnore]
    public bool? RemoveSubjectIdFromCredentialAndReplaceWithClaim { get; init; }

    /// <summary>
    /// JWTs allows to omit the ExpirationDate property of the Verifiable Credential and place it in the header as a "exp" claim instead.
    /// Settings this flag to true will remove the ExpirationDate property from the Credential. Otherwise it will be duplicated in the header.
    /// </summary>
    [JsonIgnore]
    public bool? RemoveExpirationDateFromCredentialAndReplaceWithClaim { get; init; }

    /// <summary>
    /// JWTs allows to omit the IssuanceDate property of the Verifiable Credential and place it in the header as a "nbf" claim instead.
    /// Settings this flag to true will remove the IssuanceDate property from the Credential. Otherwise it will be duplicated in the header.
    /// </summary>
    [JsonIgnore]
    public bool? RemoveIssuanceDateFromCredentialAndReplaceWithClaim { get; init; }

    /// <summary>
    /// JWTs allows to omit the Issuer property of the Verifiable Credential and place it in the header as a "iss" claim instead.
    /// Settings this flag to true will remove the Issuer property from the Credential. Otherwise it will be duplicated in the header.
    /// </summary>
    [JsonIgnore]
    public bool? RemoveIssuerOrHolderFromCredentialAndReplaceWithClaim { get; init; }

    /// <summary>
    /// JWTs allows to omit the Id property of the Verifiable Credential and place it in the header as a "jti" claim instead.
    /// Settings this flag to true will remove the Id property from the Credential. Otherwise it will be duplicated in the header.
    /// </summary>
    [JsonIgnore]
    public bool? RemoveIdFromCredentialOrPresentationAndReplaceWithClaim { get; init; }

    /// <summary>
    /// When having a Verifiable Presentation, with one or more Verifiable Credentials inside the, credentials
    /// can also be written as JWTs. This flag indicates if the credentials are written as JWTs
    /// </summary>
    [JsonIgnore]
    public bool? EmbeddedCredentialInPresentationAsJwt { get; init; }


    public virtual bool Equals(JwtParsingArtefact? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return
            JwtVerificationKeyId == other.JwtVerificationKeyId &&
            (JwtParsingWarnings == null && other.JwtParsingWarnings == null ||
             JwtParsingWarnings != null && other.JwtParsingWarnings != null &&
             JwtParsingWarnings.SequenceEqual(other.JwtParsingWarnings)) &&
            DictionaryStringObjectJsonEquals.JsonEquals(JwtAdditionalClaims, other.JwtAdditionalClaims) &&
            JwtAlg == other.JwtAlg &&
            JwtSignature == other.JwtSignature &&
            RemoveSubjectIdFromCredentialAndReplaceWithClaim == other.RemoveSubjectIdFromCredentialAndReplaceWithClaim &&
            RemoveExpirationDateFromCredentialAndReplaceWithClaim == other.RemoveExpirationDateFromCredentialAndReplaceWithClaim &&
            RemoveIssuanceDateFromCredentialAndReplaceWithClaim == other.RemoveIssuanceDateFromCredentialAndReplaceWithClaim &&
            RemoveIssuerOrHolderFromCredentialAndReplaceWithClaim == other.RemoveIssuerOrHolderFromCredentialAndReplaceWithClaim &&
            RemoveIdFromCredentialOrPresentationAndReplaceWithClaim == other.RemoveIdFromCredentialOrPresentationAndReplaceWithClaim &&
            PrivateKeyForSigningJwtCredentialsInsidePresentations == other.PrivateKeyForSigningJwtCredentialsInsidePresentations &&
            EmbeddedCredentialInPresentationAsJwt == other.EmbeddedCredentialInPresentationAsJwt;
    }

    public override int GetHashCode()
    {
        HashCode hash = new HashCode();
        hash.Add(JwtVerificationKeyId);
        if (JwtParsingWarnings != null)
        {
            foreach (var warning in JwtParsingWarnings)
            {
                hash.Add(warning);
            }
        }

        DictionaryStringObjectJsonEquals.AddToHashCode(JwtAdditionalClaims, ref hash);
        hash.Add(JwtAlg);
        hash.Add(JwtSignature);
        hash.Add(RemoveSubjectIdFromCredentialAndReplaceWithClaim);
        hash.Add(RemoveExpirationDateFromCredentialAndReplaceWithClaim);
        hash.Add(RemoveIssuanceDateFromCredentialAndReplaceWithClaim);
        hash.Add(RemoveIssuerOrHolderFromCredentialAndReplaceWithClaim);
        hash.Add(RemoveIdFromCredentialOrPresentationAndReplaceWithClaim);
        hash.Add(PrivateKeyForSigningJwtCredentialsInsidePresentations);
        hash.Add(EmbeddedCredentialInPresentationAsJwt);
        return hash.ToHashCode();
    }
}