using System.Text.Json.Serialization;
using Blocktrust.CredentialWorkflow.Core.Domain.Common;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;

public class ActionInputCredentialIssuance : ActionInput
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

