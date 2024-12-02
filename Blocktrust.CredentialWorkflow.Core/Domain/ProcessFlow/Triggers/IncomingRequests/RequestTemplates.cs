namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Triggers.IncomingRequests;

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
