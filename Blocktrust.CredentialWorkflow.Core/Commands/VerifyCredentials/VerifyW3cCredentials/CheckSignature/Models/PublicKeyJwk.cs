namespace Blocktrust.CredentialWorkflow.Core.Commands.VerifyCredentials.VerifyW3cCredentials.CheckSignature.Models;

using System.Text.Json.Serialization;

public class PublicKeyJwk
{
    [JsonPropertyName("crv")] public string Curve { get; set; }
    [JsonPropertyName("kty")] public string KeyType { get; set; }
    [JsonPropertyName("x")] public string X { get; set; }
    [JsonPropertyName("y")] public string? Y { get; set; }
}