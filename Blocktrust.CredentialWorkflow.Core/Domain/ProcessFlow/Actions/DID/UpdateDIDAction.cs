using Blocktrust.CredentialWorkflow.Core.Domain.Common;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.DID;

public class UpdateDIDAction : ActionInput
{
    // Id is already defined in the base class ActionInput
    
    [JsonPropertyName("useTenantRegistrar")]
    public bool UseTenantRegistrar { get; set; } = true;
    
    private ParameterReference _registrarUrl;
    private ParameterReference _walletId;
    private ParameterReference _did;
    
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
    
    [JsonPropertyName("did")]
    public ParameterReference Did
    {
        get => _did ?? new ParameterReference { Source = ParameterSource.Static }; 
        set => _did = value; 
    }
    
    [JsonPropertyName("updateOperations")]
    public List<DIDUpdateOperation> UpdateOperations { get; set; } = new List<DIDUpdateOperation>();
}

public class DIDUpdateOperation
{
    private ParameterReference _operationType;
    
    [JsonPropertyName("operationType")]
    public ParameterReference OperationType 
    { 
        get => _operationType ?? new ParameterReference { Source = ParameterSource.Static, DefaultValue = "Add" }; 
        set => _operationType = value; 
    }
    
    [JsonPropertyName("verificationMethod")]
    public VerificationMethod? VerificationMethod { get; set; }
    
    [JsonPropertyName("keyId")]
    public ParameterReference? KeyId { get; set; }
    
    [JsonPropertyName("services")]
    public List<ServiceEndpoint> Services { get; set; } = new List<ServiceEndpoint>();
}