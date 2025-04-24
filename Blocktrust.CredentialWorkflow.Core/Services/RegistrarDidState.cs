namespace Blocktrust.CredentialWorkflow.Core.Services;

using System.Text.Json;
using System.Text.Json.Serialization;

public sealed class RegistrarDidState
{
    [JsonPropertyName("state")] public string State { get; set; } = null!; // "wait", "finished", "failed"
    [JsonPropertyName("did")] public string? Did { get; set; }
    [JsonPropertyName("secret")] public JsonElement? Secret { get; set; }
    [JsonPropertyName("didDocument")] public JsonElement? DidDocument { get; set; }
    [JsonPropertyName("reason")] public string? Reason { get; set; }
    [JsonPropertyName("wait")] public string? Wait { get; set; }
}