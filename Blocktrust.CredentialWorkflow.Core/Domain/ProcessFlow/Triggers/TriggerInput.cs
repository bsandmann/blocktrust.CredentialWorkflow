using System.Text.Json.Serialization;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Triggers.IncomingRequests;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Triggers;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(TriggerInputIncomingRequest), typeDiscriminator: "incomingRequest")]
[JsonDerivedType(typeof(TriggerInputRecurringTimer), typeDiscriminator: "recurringTimer")]
[JsonDerivedType(typeof(TriggerInputOnDemand), typeDiscriminator: "onDemand")]
public class TriggerInput
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
}