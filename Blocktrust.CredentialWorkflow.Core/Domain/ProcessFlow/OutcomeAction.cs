using System.Text.Json.Serialization;
using Blocktrust.CredentialWorkflow.Core.Domain.Common;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow;

public class OutcomeAction : ActionInput
{
    [JsonPropertyName("type")]
    public EOutcomeActionType Type { get; set; }

    [JsonPropertyName("destination")]
    public ParameterReference Destination { get; set; } = new();

    [JsonPropertyName("content")]
    public Dictionary<string, ParameterReference> Content { get; set; } = new();
}

public enum EOutcomeActionType
{
    Log,
    Post
}