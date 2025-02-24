namespace Blocktrust.CredentialWorkflow.Core.Commands.ValidateCredentials.W3cValidation;

public class W3cValidationError
{
    public string RuleType { get; set; }
    public string Message { get; set; }

    public W3cValidationError(string ruleType, string message)
    {
        RuleType = ruleType;
        Message = message;
    }
}