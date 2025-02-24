using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Validation;
using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.ValidateCredentials.CustomValidation;

public class CustomValidationRequest : IRequest<Result<CustomValidationResult>>
{
    public object Data { get; }
    public List<CustomValidationRule> Rules { get; }

    public CustomValidationRequest(object data, List<CustomValidationRule> rules)
    {
        Data = data;
        Rules = rules;
    }
}

public class CustomValidationResult
{
    public bool IsValid { get; set; }
    public List<CustomValidationError> Errors { get; set; } = new();
}

public class CustomValidationError
{
    public string RuleName { get; set; }
    public string Message { get; set; }

    public CustomValidationError(string ruleName, string message)
    {
        RuleName = ruleName;
        Message = message;
    }
}