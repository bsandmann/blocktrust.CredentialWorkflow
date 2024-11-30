using System.Text.Json.Serialization;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Verification;

public class VerifyW3cSdCredential : ActionInput
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