using Blocktrust.CredentialWorkflow.Core.Domain.Common;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.DID;

public class CreateDIDAction : ActionInput
{
    // Id is already defined in the base class ActionInput
    
    [JsonPropertyName("useTenantRegistrar")]
    public bool UseTenantRegistrar { get; set; } = true;
    
    [JsonPropertyName("registrarUrl")]
    public ParameterReference RegistrarUrl { get; set; } = new ParameterReference { Source = ParameterSource.Static };
    
    [JsonPropertyName("walletId")]
    public ParameterReference WalletId { get; set; } = new ParameterReference { Source = ParameterSource.Static };
    
    [JsonPropertyName("verificationMethods")]
    public List<VerificationMethod> VerificationMethods { get; set; } = new List<VerificationMethod>();
    
    [JsonPropertyName("services")]
    public List<ServiceEndpoint> Services { get; set; } = new List<ServiceEndpoint>();
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

public class ServiceEndpoint
{
    [JsonPropertyName("serviceId")]
    public string ServiceId { get; set; } = "service-1";
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = "LinkedDomain";
    
    [JsonPropertyName("endpoint")]
    public string Endpoint { get; set; } = "";
}