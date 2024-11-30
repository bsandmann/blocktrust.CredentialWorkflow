using System.Text.Json.Serialization;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Triggers;

public class Trigger
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ETriggerType Type { get; set; }

    [JsonPropertyName("input")]
     public TriggerInput Input { get; set; }
}