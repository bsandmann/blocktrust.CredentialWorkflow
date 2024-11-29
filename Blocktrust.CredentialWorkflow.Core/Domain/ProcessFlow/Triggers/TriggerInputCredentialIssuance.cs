using System.Text.Json.Serialization;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Triggers;

public class TriggerInputCredentialIssuance : TriggerInput
{
    [JsonPropertyName("subjectDid")]
    public string SubjectDid { get; set; } = null!;

    [JsonPropertyName("deliveryType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EDeliveryType DeliveryType { get; set; }

    [JsonPropertyName("destination")]
    public string Destination { get; set; } = null!;

    [JsonPropertyName("additionalProperties")]
    public Dictionary<string, string> AdditionalProperties { get; set; } = new();
}
