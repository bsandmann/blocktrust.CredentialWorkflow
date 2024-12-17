namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Triggers.HttpRequestsTemplates;

public class HttpRequestTemplate
{
    public string Method { get; set; } = "POST";
    public Dictionary<string, ParameterDefinition> Parameters { get; set; } = new();
}
