using System.Text.Json.Serialization;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Action;

public class ActionInputDelivery : ActionInput
{
    [JsonPropertyName("deliveryType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EDeliveryType DeliveryType { get; set; }

    [JsonPropertyName("destination")] 
    public string Destination { get; set; } = null!; // Email or PeerDID
}
