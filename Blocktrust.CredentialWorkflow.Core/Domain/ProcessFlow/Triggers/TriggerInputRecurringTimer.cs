using System.Text.Json.Serialization;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Triggers;

public class TriggerInputRecurringTimer : TriggerInput
{
    [JsonPropertyName("cronExpression")] public string CronExpression { get; set; }
}