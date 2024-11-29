using Blocktrust.CredentialWorkflow.Core.Domain.Common;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Input;

public class ActionInputDIDCommTrustPing : ActionInput
{
    public ParameterReference TargetDid { get; set; } = new();
}