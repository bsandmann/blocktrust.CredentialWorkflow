namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Action;

using System.Text.Json.Serialization;

public class ActionInputOutgoingRequest : ActionInput
{
    [JsonPropertyName("id")] public Guid Id { get; set; }

    [JsonPropertyName("method")] public string Method { get; set; }

    [JsonPropertyName("uri")] public string Uri { get; set; }

    [JsonPropertyName("body")] public string Body { get; set; }

    [JsonPropertyName("headers")] public Dictionary<string, string> Headers { get; set; }
}