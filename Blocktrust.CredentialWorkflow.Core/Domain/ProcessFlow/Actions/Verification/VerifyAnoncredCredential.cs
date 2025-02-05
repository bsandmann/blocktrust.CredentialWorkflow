using System.Text.Json.Serialization;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Verification;

public class VerifyAnoncredCredential : ActionInput
{
    [JsonPropertyName("checkSignature")]
    public bool CheckSignature { get; set; } = true;

    [JsonPropertyName("checkStatus")]
    public bool CheckStatus { get; set; } = false;

    [JsonPropertyName("checkSchema")]
    public bool CheckSchema { get; set; } = false;

    [JsonPropertyName("checkTrustRegistry")]
    public bool CheckTrustRegistry { get; set; } = false;

    [JsonPropertyName("checkExpiry")]
    public bool CheckExpiry { get; set; } = true;

    // Anoncred specific properties can be added later
}