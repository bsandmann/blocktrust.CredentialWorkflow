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
    public ParameterReference KeyId { get; set; } = new ParameterReference { 
        Source = ParameterSource.Static, 
        DefaultValue = "key-1" 
    };
    
    [JsonPropertyName("purpose")]
    public ParameterReference Purpose { get; set; } = new ParameterReference { 
        Source = ParameterSource.Static,
        DefaultValue = "Authentication"
    };
    
    [JsonPropertyName("curve")]
    public ParameterReference Curve { get; set; } = new ParameterReference { 
        Source = ParameterSource.Static,
        DefaultValue = "Secp256k1"
    };
}

public class ServiceEndpoint
{
    [JsonPropertyName("serviceId")]
    public ParameterReference ServiceId { get; set; } = new ParameterReference { 
        Source = ParameterSource.Static,
        DefaultValue = "service-1"
    };
    
    [JsonPropertyName("type")]
    public ParameterReference Type { get; set; } = new ParameterReference { 
        Source = ParameterSource.Static,
        DefaultValue = "LinkedDomain"
    };
    
    [JsonPropertyName("endpoint")]
    public ParameterReference Endpoint { get; set; } = new ParameterReference { 
        Source = ParameterSource.Static,
        DefaultValue = ""
    };
}