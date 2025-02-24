using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Validation;
using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.ValidateCredentials.W3cValidation;

public class W3cValidationRequest : IRequest<Result<ValidationResult>>
{
    public string Credential { get; }
    public List<ValidationRule> Rules { get; }

    public W3cValidationRequest(string credential, List<ValidationRule> rules)
    {
        Credential = credential;
        Rules = rules;
    }
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationError> Errors { get; set; } = new();
}

public class ValidationError
{
    public string RuleType { get; set; }
    public string Message { get; set; }

    public ValidationError(string ruleType, string message)
    {
        RuleType = ruleType;
        Message = message;
    }
}