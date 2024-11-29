using System.Text.Json.Serialization;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Verification;

public class VerifyW3cCredential : ActionInput
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