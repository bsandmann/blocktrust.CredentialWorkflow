namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Action;

using System.Text.Json.Serialization;

public class ActionInputCredentialIssuance : ActionInput
{
    [JsonPropertyName("id")] public Guid Id { get; set; }

    [JsonPropertyName("subject")] public string Subject { get; set; }

    [JsonPropertyName("issuer")] public string Issuer { get; set; }

    [JsonPropertyName("claims")] public Dictionary<string, object>? Claims { get; set; }
}