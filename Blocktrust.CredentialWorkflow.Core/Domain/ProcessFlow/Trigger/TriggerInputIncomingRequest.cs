namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Trigger;

using System.Text.Json.Serialization;
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

// Separate class to define request templates
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
                ["deliveryMethod"] = new ParameterDefinition
                {
                    Type = ParameterType.String,
                    Required = true,
                    AllowedValues = new[] { "email", "didcomm" },
                    Description = "Delivery method for the credential"
                },
                ["destination"] = new ParameterDefinition
                {
                    Type = ParameterType.String,
                    Required = true,
                    Description = "Email address or Peer DID depending on delivery method"
                }
            }
        }
        // Other templates can be added here later
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