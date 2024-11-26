using System.Text.Json.Serialization;
using Blocktrust.CredentialWorkflow.Core.Domain.Common;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Action.Input;

public class ActionInputW3cCredential : ActionInput
{
    [JsonPropertyName("subjectDid")]
    public ParameterReference SubjectDid { get; set; } = new()
    {
        Source = ParameterSource.TriggerInput,
        Path = "subjectDid"
    };

    [JsonPropertyName("issuerDid")]
    public ParameterReference IssuerDid { get; set; } = new()
    {
        Source = ParameterSource.AppSettings,
        Path = "DefaultIssuerDid"
    };

    [JsonPropertyName("claims")]
    public Dictionary<string, ClaimValue> Claims { get; set; } = new();
}

public class ActionInputW3cSdCredential : ActionInputW3cCredential
{
    [JsonPropertyName("selectiveDisclosure")]
    public bool EnableSelectiveDisclosure { get; set; } = true;
    
    [JsonPropertyName("frameVersion")]
    public string FrameVersion { get; set; } = "1.0";
}

public class ActionInputAnoncredCredential : ActionInput
{
    [JsonPropertyName("subjectDid")]
    public ParameterReference SubjectDid { get; set; } = new();

    [JsonPropertyName("issuerDid")]
    public ParameterReference IssuerDid { get; set; } = new();

    [JsonPropertyName("credentialDefinitionId")]
    public string CredentialDefinitionId { get; set; } = "";

    [JsonPropertyName("attributes")]
    public Dictionary<string, ClaimValue> Attributes { get; set; } = new();
}