using Blocktrust.CredentialWorkflow.Core.Domain.Common;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.DID;

public class DeactivateDIDAction : ActionInput
{
    public Guid Id { get; set; }
    
    public bool UseTenantRegistrar { get; set; } = true;
    
    // Custom registrar fields (only used when UseTenantRegistrar = false)
    public ParameterReference RegistrarUrl { get; set; } = new()
    {
        Source = ParameterSource.Static,
        DefaultValue = string.Empty
    };
    
    public ParameterReference WalletId { get; set; } = new()
    {
        Source = ParameterSource.Static,
        DefaultValue = string.Empty
    };
    
    // DID to deactivate
    public ParameterReference Did { get; set; } = new()
    {
        Source = ParameterSource.Static,
        DefaultValue = string.Empty
    };
    
    // Optional master key secret for the DID
    public ParameterReference MasterKeySecret { get; set; } = new()
    {
        Source = ParameterSource.Static,
        DefaultValue = string.Empty
    };
}