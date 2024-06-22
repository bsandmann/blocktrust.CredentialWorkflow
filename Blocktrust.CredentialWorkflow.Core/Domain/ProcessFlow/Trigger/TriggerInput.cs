namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Trigger;

using System.Text.Json.Serialization;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(TriggerInput), typeDiscriminator: "base")]
[JsonDerivedType(typeof(TriggerInputIncomingRequest), typeDiscriminator: "incomingRequest")]
[JsonDerivedType(typeof(TriggerInputRecurringTimer), typeDiscriminator: "recurringTimer")]
public class TriggerInput
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
}