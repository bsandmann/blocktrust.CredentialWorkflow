namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Trigger;

using System.Text.Json.Serialization;

public class TriggerInputIncomingRequest : TriggerInput
{
    [JsonPropertyName("method")] public string Method { get; set; }

    [JsonPropertyName("uri")] public string Uri { get; set; }

    [JsonPropertyName("body")] public object Body { get; set; }

    [JsonPropertyName("headers")] public Dictionary<string, string> Headers { get; set; }
}