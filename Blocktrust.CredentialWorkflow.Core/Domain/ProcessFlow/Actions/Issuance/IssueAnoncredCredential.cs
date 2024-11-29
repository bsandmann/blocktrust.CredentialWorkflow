using System.Text.Json.Serialization;
using Blocktrust.CredentialWorkflow.Core.Domain.Common;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Issuance;

public class IssueAnoncredCredential : ActionInput
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