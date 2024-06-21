namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Trigger;

using System.Text.Json.Serialization;

public class TriggerInputOnDemand : TriggerInput
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
}