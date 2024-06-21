namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class ProcessFlow
    {
        [JsonPropertyName("triggers")] public Dictionary<Guid, Trigger.Trigger> Triggers { get; set; } = new();

        [JsonPropertyName("actions")] public Dictionary<Guid, Action.Action> Actions { get; set; } = new();

        [JsonPropertyName("outputs")] public Dictionary<Guid, Output.Output> Outputs { get; set; } = new();
    }
}