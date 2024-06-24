namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Trigger;

using System.Text.Json.Serialization;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(TriggerInputIncomingRequest), typeDiscriminator: "incomingRequest")]
[JsonDerivedType(typeof(TriggerInputRecurringTimer), typeDiscriminator: "recurringTimer")]
[JsonDerivedType(typeof(TriggerInputOnDemand), typeDiscriminator: "onDemand")]
public class TriggerInput
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
}