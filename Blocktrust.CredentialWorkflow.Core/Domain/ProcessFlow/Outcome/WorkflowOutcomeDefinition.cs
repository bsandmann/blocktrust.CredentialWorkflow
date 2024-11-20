using System.Text.Json.Serialization;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Outcome;

public class WorkflowOutcomeDefinition
{
    [JsonPropertyName("type")]
    public EOutcomeType Type { get; set; }
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}