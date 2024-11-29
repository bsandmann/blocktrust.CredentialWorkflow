using System.Text.Json.Serialization;
using Blocktrust.CredentialWorkflow.Core.Domain.Common;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Input;

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

public class ActionInputVerifyW3cCredential : ActionInput
{
    [JsonPropertyName("checkSignature")]
    public bool CheckSignature { get; set; } = true;

    [JsonPropertyName("checkStatus")]
    public bool CheckStatus { get; set; } = true;

    [JsonPropertyName("checkSchema")]
    public bool CheckSchema { get; set; } = true;

    [JsonPropertyName("checkTrustRegistry")]
    public bool CheckTrustRegistry { get; set; } = false;

    [JsonPropertyName("checkExpiry")]
    public bool CheckExpiry { get; set; } = true;

    [JsonPropertyName("requiredIssuerId")]
    public string? RequiredIssuerId { get; set; }
}



public class ActionInputVerifyW3cSdCredential : ActionInput
{
    [JsonPropertyName("checkSignature")]
    public bool CheckSignature { get; set; } = true;

    [JsonPropertyName("checkStatus")]
    public bool CheckStatus { get; set; } = true;

    [JsonPropertyName("checkSchema")]
    public bool CheckSchema { get; set; } = true;

    [JsonPropertyName("checkTrustRegistry")]
    public bool CheckTrustRegistry { get; set; } = false;

    [JsonPropertyName("checkExpiry")]
    public bool CheckExpiry { get; set; } = true;

    // SD-VC specific properties can be added later
}

public class ActionInputVerifyAnoncredCredential : ActionInput
{
    [JsonPropertyName("checkSignature")]
    public bool CheckSignature { get; set; } = true;

    [JsonPropertyName("checkStatus")]
    public bool CheckStatus { get; set; } = true;

    [JsonPropertyName("checkSchema")]
    public bool CheckSchema { get; set; } = true;

    [JsonPropertyName("checkTrustRegistry")]
    public bool CheckTrustRegistry { get; set; } = false;

    [JsonPropertyName("checkExpiry")]
    public bool CheckExpiry { get; set; } = true;

    // Anoncred specific properties can be added later
}