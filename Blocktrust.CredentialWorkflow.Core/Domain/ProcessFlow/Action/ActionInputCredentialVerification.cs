namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Action;

using System.Text.Json.Serialization;

public class ActionInputCredentialVerification : ActionInput
{
    [JsonPropertyName("id")] public Guid Id { get; set; }

    [JsonPropertyName("checkExpiration")] public bool CheckExpiration { get; set; }

    [JsonPropertyName("checkSignature")] public bool CheckSignature { get; set; }

    [JsonPropertyName("checkIssuer")] public bool CheckIssuer { get; set; }

    [JsonPropertyName("requiredIssuer")] public string? RequiredIssuer { get; set; }

    [JsonPropertyName("checkSchema")] public bool CheckSchema { get; set; }

    [JsonPropertyName("requiredSchema")] public Uri? RequiredSchema { get; set; }

    [JsonPropertyName("checkClaims")] public bool CheckClaims { get; set; }

    [JsonPropertyName("requiredClaims")] public Dictionary<string, object>? RequiredClaims { get; set; }
}