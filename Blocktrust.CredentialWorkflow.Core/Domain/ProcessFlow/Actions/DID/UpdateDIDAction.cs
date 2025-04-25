using Blocktrust.CredentialWorkflow.Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text.Json;
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

    [JsonPropertyName("updateOperations")]
    public List<DIDUpdateOperation> UpdateOperations { get; set; } = new List<DIDUpdateOperation>();
}

// Use a custom converter to ensure operation-specific fields are properly handled
[JsonConverter(typeof(DIDUpdateOperationConverter))]
public class DIDUpdateOperation
{
    private ParameterReference _operationType;

    [JsonPropertyName("operationType")]
    public ParameterReference OperationType
    {
        get => _operationType ?? new ParameterReference { Source = ParameterSource.Static, DefaultValue = "Add" };
        set => _operationType = value;
    }

    // Only serialize VerificationMethod when it's actually used (for Add operations)
    [JsonPropertyName("verificationMethod")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public VerificationMethod? VerificationMethod { get; set; }

    // Only serialize KeyId when it's actually used (for Remove operations)
    [JsonPropertyName("keyId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ParameterReference? KeyId { get; set; }

    [JsonPropertyName("services")]
    public List<ServiceEndpoint> Services { get; set; } = new List<ServiceEndpoint>();

    // Helper property to determine what kind of operation this is (only for code logic, not serialized)
    [JsonIgnore]
    public string? OperationTypeValue => OperationType?.Source == ParameterSource.Static ? OperationType.DefaultValue : null;
}

/// <summary>
/// Custom JSON converter to ensure proper serialization of DIDUpdateOperation based on operation type
/// </summary>
public class DIDUpdateOperationConverter : JsonConverter<DIDUpdateOperation>
{
    public override DIDUpdateOperation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected start of object");
        }

        var operation = new DIDUpdateOperation();
        
        // Store the original JsonElement to read all properties
        using var document = JsonDocument.ParseValue(ref reader);
        var rootElement = document.RootElement;
        
        // Read operationType
        if (rootElement.TryGetProperty("operationType", out var opTypeElement))
        {
            // Parse the operationType into ParameterReference
            operation.OperationType = JsonSerializer.Deserialize<ParameterReference>(
                opTypeElement.GetRawText(), 
                options
            ) ?? new ParameterReference { Source = ParameterSource.Static, DefaultValue = "Add" };
        }
        
        // Determine the operation type value for selective deserialization
        string? operationType = null;
        if (operation.OperationType.Source == ParameterSource.Static)
        {
            operationType = operation.OperationType.DefaultValue;
        }
        
        // Read fields based on operation type
        if (operationType == "Add" && rootElement.TryGetProperty("verificationMethod", out var vmElement))
        {
            operation.VerificationMethod = JsonSerializer.Deserialize<VerificationMethod>(
                vmElement.GetRawText(), 
                options
            );
        }
        else if (operationType == "Remove" && rootElement.TryGetProperty("keyId", out var keyIdElement))
        {
            operation.KeyId = JsonSerializer.Deserialize<ParameterReference>(
                keyIdElement.GetRawText(), 
                options
            );
        }
        
        // Always read services
        if (rootElement.TryGetProperty("services", out var servicesElement))
        {
            operation.Services = JsonSerializer.Deserialize<List<ServiceEndpoint>>(
                servicesElement.GetRawText(), 
                options
            ) ?? new List<ServiceEndpoint>();
        }
        
        return operation;
    }

    public override void Write(Utf8JsonWriter writer, DIDUpdateOperation value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        // Always write operationType
        writer.WritePropertyName("operationType");
        JsonSerializer.Serialize(writer, value.OperationType, options);
        
        // Write fields based on operation type
        if (value.OperationType.Source == ParameterSource.Static)
        {
            var operationType = value.OperationType.DefaultValue;
            
            if (operationType == "Add" && value.VerificationMethod != null)
            {
                writer.WritePropertyName("verificationMethod");
                JsonSerializer.Serialize(writer, value.VerificationMethod, options);
            }
            else if (operationType == "Remove" && value.KeyId != null)
            {
                writer.WritePropertyName("keyId");
                JsonSerializer.Serialize(writer, value.KeyId, options);
            }
        }
        
        // Always write services
        writer.WritePropertyName("services");
        JsonSerializer.Serialize(writer, value.Services, options);
        
        writer.WriteEndObject();
    }
}