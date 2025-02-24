using System.Text.Json;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Validation;
using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.ValidateCredentials.W3cValidation;

public class W3cValidationHandler : IRequestHandler<W3cValidationRequest, Result<ValidationResult>>
{
    public async Task<Result<ValidationResult>> Handle(W3cValidationRequest request, CancellationToken cancellationToken)
    {
        var result = new ValidationResult();
        try
        {
            var credentialJson = JsonDocument.Parse(request.Credential);
            
            foreach (var rule in request.Rules)
            {
                switch (rule.Type.ToLower())
                {
                    case "required":
                        ValidateRequiredField(credentialJson, rule, result);
                        break;
                    case "format":
                        ValidateFormat(credentialJson, rule, result);
                        break;
                    case "range":
                        ValidateRange(credentialJson, rule, result);
                        break;
                    case "custom":
                        ValidateCustomRule(credentialJson, rule, result);
                        break;
                }
            }

            result.IsValid = !result.Errors.Any();
            return Result.Ok(result);
        }
        catch (JsonException ex)
        {
            return Result.Fail($"Invalid credential format: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result.Fail($"Validation failed: {ex.Message}");
        }
    }

    private void ValidateRequiredField(JsonDocument credential, ValidationRule rule, ValidationResult result)
    {
        var path = rule.Configuration;
        var pathParts = path.Split('.');
        var element = credential.RootElement;

        foreach (var part in pathParts)
        {
            if (!element.TryGetProperty(part, out var child))
            {
                result.Errors.Add(new ValidationError("Required", $"Required field '{path}' is missing"));
                return;
            }
            element = child;
        }
    }

    private void ValidateFormat(JsonDocument credential, ValidationRule rule, ValidationResult result)
    {
        var config = rule.Configuration.Split(':');
        if (config.Length != 2)
        {
            result.Errors.Add(new ValidationError("Format", "Invalid format rule configuration"));
            return;
        }

        var path = config[0];
        var format = config[1];
        var pathParts = path.Split('.');
        var element = credential.RootElement;

        foreach (var part in pathParts)
        {
            if (!element.TryGetProperty(part, out var child))
            {
                result.Errors.Add(new ValidationError("Format", $"Field '{path}' not found"));
                return;
            }
            element = child;
        }

        // Validate format based on the format type
        switch (format.ToUpper())
        {
            case "ISO8601":
                if (!DateTime.TryParse(element.GetString(), out _))
                {
                    result.Errors.Add(new ValidationError("Format", $"Field '{path}' is not a valid ISO8601 date"));
                }
                break;
            // Add more format validations as needed
        }
    }

    private void ValidateRange(JsonDocument credential, ValidationRule rule, ValidationResult result)
    {
        var config = rule.Configuration.Split(':');
        if (config.Length != 2)
        {
            result.Errors.Add(new ValidationError("Range", "Invalid range rule configuration"));
            return;
        }

        var path = config[0];
        var range = config[1].Split('-');
        if (range.Length != 2)
        {
            result.Errors.Add(new ValidationError("Range", "Invalid range format"));
            return;
        }

        if (!decimal.TryParse(range[0], out var min) || !decimal.TryParse(range[1], out var max))
        {
            result.Errors.Add(new ValidationError("Range", "Invalid range values"));
            return;
        }

        var pathParts = path.Split('.');
        var element = credential.RootElement;

        foreach (var part in pathParts)
        {
            if (!element.TryGetProperty(part, out var child))
            {
                result.Errors.Add(new ValidationError("Range", $"Field '{path}' not found"));
                return;
            }
            element = child;
        }

        if (!decimal.TryParse(element.GetString(), out var value))
        {
            result.Errors.Add(new ValidationError("Range", $"Field '{path}' is not a valid number"));
            return;
        }

        if (value < min || value > max)
        {
            result.Errors.Add(new ValidationError("Range", $"Field '{path}' value {value} is outside range {min}-{max}"));
        }
    }

    private void ValidateCustomRule(JsonDocument credential, ValidationRule rule, ValidationResult result)
    {
        // Custom rule validation - can be extended based on requirements
        // For now, just log that custom validation is not implemented
        result.Errors.Add(new ValidationError("Custom", "Custom validation rules are not implemented"));
    }
}