namespace Blocktrust.VerifiableCredential.Common.JwtModels;

using System.Text.Json.Serialization;

public record JwtCredentialHeaders
{
    // 'alg' MUST be set for digital signatures, 'none' if no alg within the method
    [JsonPropertyName("alg")]
    public string Alg { get; init; } = "none"; 

    // 'kid' MAY be used for multiple keys
    [JsonPropertyName("kid")]
    public string? Kid { get; init; }

    // 'typ' MUST be set to 'JWT' if present
    [JsonPropertyName("typ")]
    public string Typ { get; init; } = "JWT";  
    
    [JsonExtensionData]
    public Dictionary<string,object> AdditionalData { get; init; } = new Dictionary<string, object>();
}