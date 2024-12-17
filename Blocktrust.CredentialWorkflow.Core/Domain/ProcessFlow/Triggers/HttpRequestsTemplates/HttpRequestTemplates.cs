namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Triggers.HttpRequestsTemplates;

public static class HttpRequestTemplates
{
    public static readonly Dictionary<string, HttpRequestTemplate> Templates = new()
    {
        ["credential-issuance"] = new HttpRequestTemplate
        {
            Method = "POST",
            Parameters = new Dictionary<string, ParameterDefinition>
            {
                ["subjectDid"] = new ParameterDefinition
                {
                    Type = ParameterType.String,
                    Required = true,
                    Description = "DID of the credential subject"
                },
                ["exampleClaim"] = new ParameterDefinition
                {
                    Type = ParameterType.String,
                    Required = false,
                    Description = "(Optional) Claims of the Credential"
                },
                ["peerDid"] = new ParameterDefinition
                {
                    Type = ParameterType.String,
                    Required = false,
                    Description = "(Optional) PeerDID of the receiving party"
                },
                ["email"] = new ParameterDefinition
                {
                    Type = ParameterType.String,
                    Required = false,
                    Description = "(Optional) Email of the receiving party"
                },
            }
        },
        ["credential-verification"] = new HttpRequestTemplate
        {
            Method = "POST",
            Parameters = new Dictionary<string, ParameterDefinition>
            {
                ["credential"] = new ParameterDefinition
                {
                    Type = ParameterType.String,
                    Required = true,
                    Description = "The credential to verify"
                }
            }
        }
    };
}
