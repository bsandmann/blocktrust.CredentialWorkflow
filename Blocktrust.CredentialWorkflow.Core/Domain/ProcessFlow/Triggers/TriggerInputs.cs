﻿using System.Text.Json.Serialization;

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

    [JsonPropertyName("messageType")]
    public string MessageType { get; set; } = "BasicMessage";

    [JsonPropertyName("peerDid")]
    public string PeerDid { get; set; } = string.Empty;

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

public class TriggerInputForm : TriggerInput
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("parameters")]
    public Dictionary<string, ParameterDefinition> Parameters { get; set; }

    public TriggerInputForm()
    {
        Id = Guid.NewGuid();
        Parameters = new Dictionary<string, ParameterDefinition>();
    }
}