using System.Text.Json.Serialization;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Triggers.IncomingRequests;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Triggers;

public class TriggerInputPresetTimer : TriggerInput
{
    [JsonPropertyName("triggerTime")] 
    public DateTime TriggerTime { get; set; }
}

public class TriggerInputWalletInteraction : TriggerInput
{
    [JsonPropertyName("walletAction")]
    public string WalletAction { get; set; } = string.Empty;

    [JsonPropertyName("requiredParameters")]
    public Dictionary<string, ParameterDefinition> RequiredParameters { get; set; } = new();
}

public class TriggerInputManual : TriggerInput
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("requiredParameters")]
    public Dictionary<string, ParameterDefinition> RequiredParameters { get; set; } = new();
}

public class TriggerInputCustomIncoming : TriggerInputIncomingRequest
{
    [JsonPropertyName("customValidation")]
    public bool EnableCustomValidation { get; set; }

    [JsonPropertyName("validationRules")]
    public Dictionary<string, string> ValidationRules { get; set; } = new();
}