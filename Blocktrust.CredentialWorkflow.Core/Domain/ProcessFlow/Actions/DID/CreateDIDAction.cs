using Blocktrust.CredentialWorkflow.Core.Domain.Common;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.DID;

public class CreateDIDAction : ActionInput
{
    // Id is already defined in the base class ActionInput
    
    [JsonPropertyName("verificationMethods")]
    public List<VerificationMethod> VerificationMethods { get; set; } = new List<VerificationMethod>();
}

public class VerificationMethod
{
    [JsonPropertyName("keyId")]
    public string KeyId { get; set; } = "key-1";
    
    [JsonPropertyName("purpose")]
    public string Purpose { get; set; } = "Authentication";
    
    [JsonPropertyName("curve")]
    public string Curve { get; set; } = "Secp256k1";
}