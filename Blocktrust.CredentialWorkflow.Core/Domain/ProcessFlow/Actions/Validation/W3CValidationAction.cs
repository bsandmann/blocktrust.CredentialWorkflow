using Blocktrust.CredentialWorkflow.Core.Domain.Common;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Validation;

public class W3cValidationAction : ActionInput
{
    public ParameterReference CredentialReference { get; set; } = new();
    public List<ValidationRule> ValidationRules { get; set; } = new();
    public string FailureAction { get; set; } = "Stop";
    public Guid? SkipToActionId { get; set; }
    public string ErrorMessageTemplate { get; set; } = string.Empty;
}

public class ValidationRule
{
    public string Type { get; set; } = string.Empty;
    public string Configuration { get; set; } = string.Empty;
}