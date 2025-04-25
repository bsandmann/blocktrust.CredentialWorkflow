namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.DID;

// DeleteDIDAction inherits from DeactivateDIDAction to simplify the codebase
// This allows us to use the UI term "Delete" while implementing it as "Deactivate"
public class DeleteDIDAction : DeactivateDIDAction
{
}