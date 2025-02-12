using System.Text.Json.Serialization;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Triggers;
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(TriggerInputHttpRequest), typeDiscriminator: "incomingRequest")]
[JsonDerivedType(typeof(TriggerInputRecurringTimer), typeDiscriminator: "recurringTimer")]
[JsonDerivedType(typeof(TriggerInputOnDemand), typeDiscriminator: "onDemand")]
[JsonDerivedType(typeof(TriggerInputForm), typeDiscriminator: "form")]  
public class TriggerInput
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
}