using System.Text.Json;
using Blocktrust.CredentialWorkflow.Core.Services;
using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.ValidateCredentials.CustomValidation;

public class CustomValidationHandler : IRequestHandler<CustomValidationRequest, Result<CustomValidationResult>>
{
    public async Task<Result<CustomValidationResult>> Handle(CustomValidationRequest request, CancellationToken cancellationToken)
    {
        var result = new CustomValidationResult();
    
        try
        {
            // Check if data is null
            if (request.Data == null)
            {
                return Result.Fail<CustomValidationResult>("Validation failed: Data cannot be null");
            }

            // Get the data
            var data = request.Data;

            foreach (var rule in request.Rules)
            {
                // Use the ValidationUtility to evaluate each rule
                var validationResult = ValidationUtility.ValidateJavaScriptExpression(data, rule);
            
                if (!validationResult.IsValid)
                {
                    result.Errors.Add(new CustomValidationError(
                        rule.Name,
                        validationResult.ErrorMessage ?? rule.ErrorMessage
                    ));
                }
            }

            result.IsValid = !result.Errors.Any();
            return Result.Ok(result);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Validation failed: {ex.Message}");
        }
    }
    
}