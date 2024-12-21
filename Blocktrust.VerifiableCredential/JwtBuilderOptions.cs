namespace Blocktrust.VerifiableCredential;

public record JwtBuilderOptions
{
    /// <summary>
    /// Do not build a signature (JWS) for the JWT. When not having a signature, the JWT needs the proof-property in the credential.
    /// </summary>
    public bool BuildJwtWithoutSignature { get; init; }

    /// <summary>
    /// If we build a JWS, we can omit the proof from the credential
    /// The issuer MAY include both a JWS and a proof property. For backward compatibility reasons, the issuer MUST use JWS to represent proofs based on a digital signature.
    /// </summary>
    public bool OmitProofFromCredentialOrPresentation { get; init; }

    /// <summary>
    /// Is is only for signing the JWT for VerifiableCredentials inside a VerifiablePresentation
    /// If provided and BuildJwtWithoutSignature is false, the VerifiableCredentials inside the VerifiablePresentation will be written as JWTs and signed with this key.
    /// </summary>
    public string? PrivateKeyForJwtSigningOfEmbeddedCredentialsInsidePresentation { get; init; }
}