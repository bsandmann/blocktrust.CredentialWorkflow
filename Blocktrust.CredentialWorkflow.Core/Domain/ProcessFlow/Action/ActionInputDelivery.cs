using System.Text.Json.Serialization;using Blocktrust.CredentialWorkflow.Core.Domain.Common;
using System.Text.Json.Serialization;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Action;

public class ActionInputDelivery : ActionInput
{
    [JsonPropertyName("deliveryType")]
    public ParameterReference DeliveryType { get; set; } = new();

    [JsonPropertyName("destination")]
    public ParameterReference Destination { get; set; } = new();
}