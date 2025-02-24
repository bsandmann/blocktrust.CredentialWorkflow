using Blocktrust.CredentialWorkflow.Core.Domain.Common;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Validation;

public class CustomValidationAction : ActionInput
{
    public ParameterReference DataReference { get; set; } = new();
    public List<CustomValidationRule> ValidationRules { get; set; } = new();
    public string FailureAction { get; set; } = "Stop";
    public Guid? SkipToActionId { get; set; }
}

public class CustomValidationRule
{
    public string Name { get; set; } = string.Empty;
    public string Expression { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}