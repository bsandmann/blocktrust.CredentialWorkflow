using System.Text.Json.Serialization;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Triggers;

public class TriggerInputIncomingRequest : TriggerInput
{
    [JsonPropertyName("endpoint")]
    public string Endpoint { get; set; } = string.Empty;

    [JsonPropertyName("method")]
    public string Method { get; set; } = "POST";

    [JsonPropertyName("template")] 
    public string Template { get; set; } = "credential-issuance"; 

    [JsonPropertyName("parameters")]
    public Dictionary<string, ParameterDefinition> Parameters { get; set; } = new();
}

public static class RequestTemplates
{
    public static readonly Dictionary<string, RequestTemplate> Templates = new()
    {
        ["credential-issuance"] = new RequestTemplate
        {
            Endpoint = "/credentials/issue/{workflowId}",
            Method = "POST",
            Parameters = new Dictionary<string, ParameterDefinition>
            {
                ["subjectDid"] = new ParameterDefinition 
                { 
                    Type = ParameterType.String, 
                    Required = true,
                    Description = "DID of the credential subject"
                },
        
            }
        },
        ["credential-verification"] = new RequestTemplate
        {
            Endpoint = "/credentials/verify/{workflowId}",
            Method = "POST",
            Parameters = new Dictionary<string, ParameterDefinition>
            {
                ["credential"] = new ParameterDefinition 
                { 
                    Type = ParameterType.String, 
                    Required = true,
                    Description = "The credential to verify"
                },
                ["verificationMethods"] = new ParameterDefinition
                {
                    Type = ParameterType.String,
                    Required = true,
                    AllowedValues = new[] { "signature", "expiry", "revocation", "trustRegistry" },
                    Description = "Verification methods to apply"
                }
            }
        }
    };
}
public class RequestTemplate
{
    public string Endpoint { get; set; } = string.Empty;
    public string Method { get; set; } = "POST";
    public Dictionary<string, ParameterDefinition> Parameters { get; set; } = new();
}

public class ParameterDefinition
{
    [JsonPropertyName("type")]
    public ParameterType Type { get; set; }

    [JsonPropertyName("required")]
    public bool Required { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("allowedValues")]
    public string[]? AllowedValues { get; set; }
}

public enum ParameterType
{
    String,
    Number,
    Boolean
}