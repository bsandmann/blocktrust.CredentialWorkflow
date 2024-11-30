using System.Text.Json.Serialization;

namespace Blocktrust.CredentialWorkflow.Core.Domain.Did;

public class DidDocument
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("verificationMethod")]
    public List<VerificationMethod> VerificationMethods { get; set; } = new();

    [JsonPropertyName("services")]
    public List<Service> Services { get; set; } = new();
}

public class VerificationMethod
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("controller")]
    public string Controller { get; set; } = string.Empty;

    [JsonPropertyName("publicKeyJwk")]
    public Dictionary<string, string> PublicKeyJwk { get; set; } = new();
}

public class Service
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("serviceEndpoint")]
    public string ServiceEndpoint { get; set; } = string.Empty;
}