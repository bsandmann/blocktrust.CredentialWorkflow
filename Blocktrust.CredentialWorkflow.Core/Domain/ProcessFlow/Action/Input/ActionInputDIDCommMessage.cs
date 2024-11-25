
using Blocktrust.CredentialWorkflow.Core.Domain.Common;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Action.Input;

public class ActionInputDIDCommMessage : ActionInput
{
    public ParameterReference TargetDid { get; set; } = new();
    public Dictionary<string, MessageFieldValue> MessageContent { get; set; } = new();
}