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
    private ParameterReference _masterKeySecret;
    private ParameterReference _network;
    
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
    
    [JsonPropertyName("masterKeySecret")]
    public ParameterReference MasterKeySecret
    {
        get => _masterKeySecret ?? new ParameterReference { Source = ParameterSource.Static }; 
        set => _masterKeySecret = value; 
    }
    
    [JsonPropertyName("network")]
    public ParameterReference Network
    {
        get => _network ?? new ParameterReference { Source = ParameterSource.Static }; 
        set => _network = value; 
    }
    
    [JsonPropertyName("operations")]
    public List<DIDDocumentOperation> Operations { get; set; } = new List<DIDDocumentOperation>();
    
    [JsonPropertyName("verificationMethods")]
    public List<VerificationMethod> VerificationMethods { get; set; } = new List<VerificationMethod>();
}

public class DIDDocumentOperation
{
    private ParameterReference _operationType;
    
    [JsonPropertyName("operationType")]
    public ParameterReference OperationType 
    { 
        get => _operationType ?? new ParameterReference { Source = ParameterSource.Static, DefaultValue = "setDidDocument" }; 
        set => _operationType = value; 
    }
    
    [JsonPropertyName("document")]
    public DIDDocument Document { get; set; } = new DIDDocument();
}

public class DIDDocument
{
    [JsonPropertyName("services")]
    public List<ServiceEndpoint> Services { get; set; } = new List<ServiceEndpoint>();
    
    [JsonPropertyName("verificationMethods")]
    public List<DIDVerificationMethodReference> VerificationMethods { get; set; } = new List<DIDVerificationMethodReference>();
}

public class DIDVerificationMethodReference
{
    private ParameterReference _id;
    
    [JsonPropertyName("id")]
    public ParameterReference Id
    {
        get => _id ?? new ParameterReference { Source = ParameterSource.Static }; 
        set => _id = value; 
    }
}