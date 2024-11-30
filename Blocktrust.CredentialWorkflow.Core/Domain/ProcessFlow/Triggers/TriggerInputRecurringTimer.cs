using System.Text.Json.Serialization;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Triggers;

public class TriggerInputRecurringTimer : TriggerInput
{
    [JsonPropertyName("timespan")] public TimeSpan TimeSpan { get; set; }
}