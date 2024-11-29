using System.Text.Json.Serialization;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Issuance;

public class IssueW3CSdCredential : IssueW3cCredential
{
    [JsonPropertyName("selectiveDisclosure")]
    public bool EnableSelectiveDisclosure { get; set; } = true;
    
    [JsonPropertyName("frameVersion")]
    public string FrameVersion { get; set; } = "1.0";
}
