namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Outgoing;

using System.Text.Json.Serialization;
using Common;

public class EmailAction : ActionInput
{
    [JsonPropertyName("to")]
    public ParameterReference To { get; set; } = new();
    
    [JsonPropertyName("subject")]
    public ParameterReference Subject { get; set; } = new();
    
    [JsonPropertyName("body")]
    public ParameterReference Body { get; set; } = new();
    
    [JsonPropertyName("attachments")]
    public List<ParameterReference> Attachments { get; set; } = new();
}