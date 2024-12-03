using System.Text.Json.Serialization;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Triggers.IncomingRequests;

public class ParameterDefinition
{
    [JsonPropertyName("type")]
    public ParameterType Type { get; set; }
    
    [JsonPropertyName("required")]
    public bool Required { get; set; }
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = "";
    
    [JsonPropertyName("allowedValues")]
    public string[]? AllowedValues { get; set; }

    [JsonPropertyName("defaultValue")]
    public string? DefaultValue { get; set; }
}
public enum ParameterType
{
    String,
    Number,
    Boolean
}