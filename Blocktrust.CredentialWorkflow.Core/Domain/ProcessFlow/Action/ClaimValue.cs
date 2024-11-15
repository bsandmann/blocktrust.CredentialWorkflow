using System.Text.Json.Serialization;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Action;

public class ClaimValue
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ClaimValueType Type { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}


public enum ClaimValueType
{
    Static,
    TriggerProperty
}