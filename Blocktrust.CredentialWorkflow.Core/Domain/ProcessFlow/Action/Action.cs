namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Action;

using System.Text.Json.Serialization;

public class Action
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EActionType Type { get; set; }

    [JsonPropertyName("input")] public ActionInput Input { get; set; }

    [JsonPropertyName("runAfter")] public Dictionary<Guid, List<EFlowStatus>> RunAfter { get; set; }
}