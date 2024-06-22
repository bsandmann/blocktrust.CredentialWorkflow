namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Trigger;

using System.Text.Json.Serialization;

public class TriggerInputRecurringTimer : TriggerInput
{
    [JsonPropertyName("timespan")] public TimeSpan TimeSpan { get; set; }
}