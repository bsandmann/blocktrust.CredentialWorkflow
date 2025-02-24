namespace Blocktrust.CredentialWorkflow.Core.Commands.ValidateCredentials.W3cValidation;

public class W3cValidationResponse
{
    public bool IsValid { get; set; }
    public List<W3cValidationError> Errors { get; set; } = new();
}