using Blocktrust.CredentialWorkflow.Core.Domain.Common;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Action.Input;

public class MessageFieldValue
{
    public ParameterSource Source { get; set; }
    public string Value { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
}