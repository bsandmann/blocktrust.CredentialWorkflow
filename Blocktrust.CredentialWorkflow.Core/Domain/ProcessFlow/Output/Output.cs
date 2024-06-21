namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Output;

using System.Text.Json.Serialization;

public class Output
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OutputType Type { get; set; }

    [JsonPropertyName("id")] public Guid Id { get; set; }

    [JsonPropertyName("value")] public object Value { get; set; }
}