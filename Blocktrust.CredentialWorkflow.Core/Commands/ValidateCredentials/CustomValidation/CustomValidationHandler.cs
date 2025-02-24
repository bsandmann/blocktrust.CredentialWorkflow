using System.Text.Json;
using FluentResults;
using Jint;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.ValidateCredentials.CustomValidation;

public class CustomValidationHandler : IRequestHandler<CustomValidationRequest, Result<CustomValidationResult>>
{
    public async Task<Result<CustomValidationResult>> Handle(CustomValidationRequest request, CancellationToken cancellationToken)
    {
        var result = new CustomValidationResult();
        var engine = new Engine();
        
        try
        {
            // Convert data to JSON string for JavaScript engine
            var dataJson = JsonSerializer.Serialize(request.Data);
            
            // Set up the JavaScript environment
            engine.SetValue("data", JsonSerializer.Deserialize<JsonElement>(dataJson));

            foreach (var rule in request.Rules)
            {
                try
                {
                    // Execute each validation rule
                    var isValid = engine.Evaluate(rule.Expression).AsBoolean();
                    
                    if (!isValid)
                    {
                        result.Errors.Add(new CustomValidationError(
                            rule.Name,
                            rule.ErrorMessage ?? $"Validation rule '{rule.Name}' failed"
                        ));
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add(new CustomValidationError(
                        rule.Name,
                        $"Error evaluating rule: {ex.Message}"
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