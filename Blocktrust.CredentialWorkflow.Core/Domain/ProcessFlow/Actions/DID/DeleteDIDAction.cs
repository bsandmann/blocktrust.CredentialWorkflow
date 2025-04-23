using Blocktrust.CredentialWorkflow.Core.Domain.Common;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.DID;

public class DeleteDIDAction : ActionInput
{
    public Guid Id { get; set; }
    
    // Empty for now, we'll add configurable properties later
}