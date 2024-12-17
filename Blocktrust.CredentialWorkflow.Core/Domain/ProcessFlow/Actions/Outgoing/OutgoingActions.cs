using System.Text.Json.Serialization;
using Blocktrust.CredentialWorkflow.Core.Domain.Common;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Outgoing;

public class DIDCommAction : ActionInput
{
    [JsonPropertyName("type")]
    public EDIDCommType Type { get; set; }
    
    [JsonPropertyName("peerDid")]
    public ParameterReference PeerDid { get; set; } = new();
    
    [JsonPropertyName("message")]
    public Dictionary<string, MessageFieldValue> MessageContent { get; set; } = new();
}

public enum EDIDCommType
{
    TrustPing,
    Message
}

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