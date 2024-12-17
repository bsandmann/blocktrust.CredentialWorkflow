namespace Blocktrust.CredentialWorkflow.Core.Domain.Common;

public class MessageFieldValue
{
    public ParameterSource Source { get; set; }
    public string Value { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
}