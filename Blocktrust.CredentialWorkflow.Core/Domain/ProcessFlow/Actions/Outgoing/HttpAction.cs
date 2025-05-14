using System.Text.Json.Serialization;
using Blocktrust.CredentialWorkflow.Core.Domain.Common;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Outgoing;

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
    
    [JsonPropertyName("parameters")]
    public Dictionary<string, ParameterReference> Parameters { get; set; } = new();
    
    public HttpAction() 
    {
        Id = Guid.NewGuid();
        Endpoint = new ParameterReference { Source = ParameterSource.Static };
        Headers = new Dictionary<string, ParameterReference>();
        Body = new Dictionary<string, ParameterReference>();
        Parameters = new Dictionary<string, ParameterReference>();  
    }
}