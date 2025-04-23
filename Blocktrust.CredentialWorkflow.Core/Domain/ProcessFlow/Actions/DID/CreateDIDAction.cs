using Blocktrust.CredentialWorkflow.Core.Domain.Common;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.DID;

public class CreateDIDAction : ActionInput
{
    // Id is already defined in the base class ActionInput
    
    [JsonPropertyName("useTenantRegistrar")]
    public bool UseTenantRegistrar { get; set; } = true;
    
    private ParameterReference _registrarUrl;
    private ParameterReference _walletId;
    
    [JsonPropertyName("registrarUrl")]
    public ParameterReference RegistrarUrl 
    { 
        get => _registrarUrl ?? new ParameterReference { Source = ParameterSource.Static }; 
        set => _registrarUrl = value; 
    }
    
    [JsonPropertyName("walletId")]
    public ParameterReference WalletId 
    { 
        get => _walletId ?? new ParameterReference { Source = ParameterSource.Static }; 
        set => _walletId = value; 
    }
    
    [JsonPropertyName("verificationMethods")]
    public List<VerificationMethod> VerificationMethods { get; set; } = new List<VerificationMethod>();
    
    [JsonPropertyName("services")]
    public List<ServiceEndpoint> Services { get; set; } = new List<ServiceEndpoint>();
}

public class VerificationMethod
{
    private ParameterReference _keyId;
    private ParameterReference _purpose;
    private ParameterReference _curve;
    
    [JsonPropertyName("keyId")]
    public ParameterReference KeyId 
    { 
        get => _keyId ?? new ParameterReference { Source = ParameterSource.Static, DefaultValue = "key-1" }; 
        set => _keyId = value; 
    }
    
    [JsonPropertyName("purpose")]
    public ParameterReference Purpose 
    { 
        get => _purpose ?? new ParameterReference { Source = ParameterSource.Static, DefaultValue = "Authentication" }; 
        set => _purpose = value; 
    }
    
    [JsonPropertyName("curve")]
    public ParameterReference Curve 
    { 
        get => _curve ?? new ParameterReference { Source = ParameterSource.Static, DefaultValue = "Secp256k1" }; 
        set => _curve = value; 
    }
}

public class ServiceEndpoint
{
    private ParameterReference _serviceId;
    private ParameterReference _type;
    private ParameterReference _endpoint;
    
    [JsonPropertyName("serviceId")]
    public ParameterReference ServiceId 
    { 
        get => _serviceId ?? new ParameterReference { Source = ParameterSource.Static, DefaultValue = "service-1" }; 
        set => _serviceId = value; 
    }
    
    [JsonPropertyName("type")]
    public ParameterReference Type 
    { 
        get => _type ?? new ParameterReference { Source = ParameterSource.Static, DefaultValue = "LinkedDomain" }; 
        set => _type = value; 
    }
    
    [JsonPropertyName("endpoint")]
    public ParameterReference Endpoint 
    { 
        get => _endpoint ?? new ParameterReference { Source = ParameterSource.Static, DefaultValue = "" }; 
        set => _endpoint = value; 
    }
}