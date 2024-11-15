namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Action;

using System.Text.Json.Serialization;

public class ActionInputCredentialIssuance : ActionInput
{
    [JsonPropertyName("subjectDid")] 
    public string SubjectDid { get; set; } = null!;

    [JsonPropertyName("issuerDid")] 
    public string IssuerDid { get; set; } = null!;

    [JsonPropertyName("claims")] 
    public Dictionary<string, ClaimValue> Claims { get; set; } = new();
}