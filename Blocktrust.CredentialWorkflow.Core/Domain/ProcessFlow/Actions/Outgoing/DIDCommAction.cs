namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Outgoing;

using System.Text.Json.Serialization;
using Common;

public class DIDCommAction : ActionInput
{
    [JsonPropertyName("type")]
    public EDIDCommType Type { get; set; }

    [JsonPropertyName("senderPeerDID")]
    public ParameterReference SenderPeerDid { get; set; } = new();

    [JsonPropertyName("recipientPeerDID")]
    public ParameterReference RecipientPeerDid { get; set; } = new();
    
    [JsonPropertyName("message")]
    public Dictionary<string, MessageFieldValue> MessageContent { get; set; } = new();
}