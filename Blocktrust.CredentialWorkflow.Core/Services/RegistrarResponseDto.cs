namespace Blocktrust.CredentialWorkflow.Core.Services;

using System.Text.Json;
using System.Text.Json.Serialization;

public sealed class RegistrarResponseDto
{
    [JsonPropertyName("jobId")] public string? JobId { get; set; }
    [JsonPropertyName("didState")] public RegistrarDidState DidState { get; set; } = null!;
    [JsonPropertyName("didRegistrationMetadata")] public JsonElement? DidRegistrationMetadata { get; set; }
    [JsonPropertyName("didDocumentMetadata")] public JsonElement? DidDocumentMetadata { get; set; }
}