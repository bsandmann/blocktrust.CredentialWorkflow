namespace Blocktrust.VerifiableCredential.Common.JwtModels;

using System.Text.Json.Serialization;

public record JwtCredentialClaims
{
    // 'exp' MUST represent expirationDate as UNIX timestamp
    [JsonPropertyName("exp")]
    public long? Exp { get; init; }

    // 'iss' MUST represent issuer or holder property
    [JsonPropertyName("iss")]
    public string Iss { get; init; }

    // 'nbf' MUST represent issuanceDate as UNIX timestamp
    [JsonPropertyName("nbf")]
    public long? Nbf { get; init; }

    // 'jti' MUST represent the id property
    [JsonPropertyName("jti")]
    public string? Jti { get; init; }

    // 'sub' MUST represent the id in credentialSubject (absent in bearer credentials)
    [JsonPropertyName("sub")]
    public string? Sub { get; init; }

    // 'aud' MUST represent the intended audience
    [JsonPropertyName("aud")]
    public string? Aud { get; init; } 
    
    [JsonPropertyName("vc")]
    public List<VerifiableCredential>? VerifiableCredentials { get; init; }
    
    [JsonPropertyName("vp")]
    public List<VerifiablePresentation>? VerifiablePresentations { get; init; }
    
    [JsonExtensionData]
    public Dictionary<string,object>? AdditionalClaims { get; init; }
    
    /// <summary>
    /// In case the Credentials inside the VerifiableCredentials property should be written as JWT inside a VerifiablePresentation,
    /// the Algorithm and PrivateKey properties should be set. E.g. "ES256"  (or "none" for unsigned JWTs)
    /// </summary>
    [JsonIgnore]
    public string? Algorithm { get; init; }
    
    /// <summary>
    /// In case the Credentials inside the VerifiableCredentials property should be a signed JWT inside a VerifiablePresentation,
    /// the Algorithm and PrivateKey properties should be set. E.g. "ES256" 
    /// </summary>
    [JsonIgnore]
    public string? PrivateKey { get; init; }
    
    /// <summary>
    /// In case the Credentials inside the VerifiableCredentials property should be a JWT without a signature inside a VerifiablePresentation,
    /// this should be set to true.
    /// </summary>
    [JsonIgnore]
    public bool? BuildJwtWithoutSignature { get; init; }
}