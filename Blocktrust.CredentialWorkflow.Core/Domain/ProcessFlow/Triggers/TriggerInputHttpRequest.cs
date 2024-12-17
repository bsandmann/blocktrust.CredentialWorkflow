namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Triggers;

using System.Text.Json.Serialization;

public class TriggerInputHttpRequest : TriggerInput
{
    [JsonPropertyName("method")]
    public string Method { get; set; } = "POST";

    [JsonPropertyName("parameters")]
    public Dictionary<string, ParameterDefinition> Parameters { get; set; } = new();
}



