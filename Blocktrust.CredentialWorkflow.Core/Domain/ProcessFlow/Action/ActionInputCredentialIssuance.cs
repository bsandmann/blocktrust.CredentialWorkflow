namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Action;

using System.Text.Json.Serialization;

public class ActionInputCredentialIssuance : ActionInput
{
    [JsonPropertyName("subject")] public string Subject { get; set; }

    [JsonPropertyName("issuer")] public string Issuer { get; set; }

    // Todo That should actually be a dictionary of string, object, but problems in the interface
    [JsonPropertyName("claims")] public Dictionary<string, string>? Claims { get; set; }
}