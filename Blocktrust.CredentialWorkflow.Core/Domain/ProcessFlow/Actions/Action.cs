using System.Text.Json.Serialization;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;

public class Action
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EActionType Type { get; set; }

    [JsonPropertyName("input")] public ActionInput Input { get; set; }

    [JsonPropertyName("runAfter")] public Dictionary<Guid, EFlowStatus> RunAfter { get; set; }
}