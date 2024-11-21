using System.Text.Json.Serialization;
using Blocktrust.CredentialWorkflow.Core.Domain.Common;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Action;

public class ClaimValue
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ClaimValueType Type { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; } = "";

    [JsonPropertyName("parameterReference")]
    public ParameterReference? ParameterReference { get; set; }
}

public enum ClaimValueType
{
    Static,
    TriggerProperty
}