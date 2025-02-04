using System.Text.Json;
using System.Text.Json.Serialization;
using Blocktrust.CredentialWorkflow.Core.Domain.Workflow;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow;

using Actions;

public class ProcessFlow
{
    [JsonPropertyName("triggers")] public Dictionary<Guid, Triggers.Trigger> Triggers { get; set; } = new();

    [JsonPropertyName("actions")] public Dictionary<Guid, Action> Actions { get; set; } = new();


    public void AddTrigger(Triggers.Trigger trigger)
    {
        if (Triggers.Any())
        {
            throw new InvalidOperationException("Only a single trigger can be added to a ProcessFlow.");
        }

        var triggerId = Guid.NewGuid();
        Triggers.Add(triggerId, trigger);
    }

    public void AddAction(Actions.Action action)
    {
        if (!Triggers.Any())
        {
            throw new InvalidOperationException("A trigger must be added before adding any actions.");
        }

        var actionId = Guid.NewGuid();
        if (!Actions.Any())
        {
            action.RunAfter = new Dictionary<Guid, EFlowStatus>
            {
                { Triggers.Keys.First(), EFlowStatus.Succeeded }
            };
        }
        else
        {
            var previousActionId = Actions.Keys.Last();
            action.RunAfter = new Dictionary<Guid, EFlowStatus>
            {
                { previousActionId, EFlowStatus.Succeeded }
            };
        }

        Actions.Add(actionId, action);
    }

    public void RemoveLastAction()
    {
        if (!Actions.Any())
        {
            throw new InvalidOperationException("There are no actions to remove.");
        }

        var lastActionId = Actions.Keys.Last();
        Actions.Remove(lastActionId);
    }

    private static JsonSerializerOptions GetJsonSerializerOptions()
    {
        return new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            IgnoreReadOnlyProperties = true,
            PropertyNameCaseInsensitive = true
        };
    }

    public string SerializeToJson()
    {
        return JsonSerializer.Serialize(this, GetJsonSerializerOptions());
    }

    public static ProcessFlow DeserializeFromJson(string json)
    {
        var processFlow = JsonSerializer.Deserialize<ProcessFlow>(json, GetJsonSerializerOptions());
        return processFlow ?? throw new ArgumentException("Invalid JSON string");
    }
}