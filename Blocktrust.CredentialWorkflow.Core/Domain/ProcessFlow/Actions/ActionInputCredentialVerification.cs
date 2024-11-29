using System.Text.Json.Serialization;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;

public class ActionInputCredentialVerification : ActionInput
{
    [JsonPropertyName("checkExpiration")] public bool CheckExpiration { get; set; }

    [JsonPropertyName("checkSignature")] public bool CheckSignature { get; set; }

    [JsonPropertyName("checkIssuer")] public bool CheckIssuer { get; set; }

    [JsonPropertyName("requiredIssuer")] public string? RequiredIssuer { get; set; }

    [JsonPropertyName("checkSchema")] public bool CheckSchema { get; set; }

    [JsonPropertyName("requiredSchema")] public Uri? RequiredSchema { get; set; }

    [JsonPropertyName("checkClaims")] public bool CheckClaims { get; set; }
    
    // Todo That should actually be a dictionary of string, object, but problems in the interface

    [JsonPropertyName("requiredClaims")] public Dictionary<string, string>? RequiredClaims { get; set; }
}