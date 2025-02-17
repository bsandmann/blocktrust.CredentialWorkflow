namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Outgoing;

using System.Text.Json.Serialization;
using Common;

public class HttpAction : ActionInput
{
    [JsonPropertyName("method")]
    public string Method { get; set; } = "POST";
    
    [JsonPropertyName("endpoint")]
    public ParameterReference Endpoint { get; set; } = new();
    
    [JsonPropertyName("headers")]
    public Dictionary<string, ParameterReference> Headers { get; set; } = new();
    
    [JsonPropertyName("body")]
    public Dictionary<string, ParameterReference> Body { get; set; } = new();
}