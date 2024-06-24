using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Action;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Trigger;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow
{
    public class ProcessFlow
    {
        [JsonPropertyName("triggers")] 
        public Dictionary<Guid, Trigger.Trigger> Triggers { get; set; } = new();

        [JsonPropertyName("actions")] 
        public Dictionary<Guid, Action.Action> Actions { get; set; } = new();

        public void AddTrigger(Trigger.Trigger trigger)
        {
            if (Triggers.Any())
            {
                throw new InvalidOperationException("Only a single trigger can be added to a ProcessFlow.");
            }

            var triggerId = Guid.NewGuid();
            Triggers.Add(triggerId, trigger);
        }

        public void AddAction(Action.Action action)
        {
            if (!Triggers.Any())
            {
                throw new InvalidOperationException("A trigger must be added before adding any actions.");
            }

            var actionId = Guid.NewGuid();

            if (!Actions.Any())
            {
                // This is the first action, so it should run after the trigger
                action.RunAfter = new Dictionary<Guid, List<EFlowStatus>>
                {
                    { Triggers.Keys.First(), new List<EFlowStatus> { EFlowStatus.Succeeded } }
                };
            }
            else
            {
                // This action should run after the previous action
                var previousActionId = Actions.Keys.Last();
                action.RunAfter = new Dictionary<Guid, List<EFlowStatus>>
                {
                    { previousActionId, new List<EFlowStatus> { EFlowStatus.Succeeded } }
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
}