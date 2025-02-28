﻿using System.Text.Json;
using System.Text.RegularExpressions;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Validation;
using Blocktrust.CredentialWorkflow.Core.Services;
using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.ValidateCredentials.W3cValidation;

/// <summary>
/// Handler for validating W3C credentials against specified validation rules
/// </summary>
public class W3cValidationHandler : IRequestHandler<W3cValidationRequest, Result<W3cValidationResponse>>
{
    private readonly CredentialParser _credentialParser;

    public W3cValidationHandler(CredentialParser credentialParser)
    {
        _credentialParser = credentialParser;
    }

    public async Task<Result<W3cValidationResponse>> Handle(W3cValidationRequest request, CancellationToken cancellationToken)
    {
        // Parse credential string (handles both JWT and JSON formats)
        var parseResult = _credentialParser.ParseCredential(request.Credential);
        if (parseResult.IsFailed)
        {
            return Result.Fail<W3cValidationResponse>($"Failed to parse credential: {parseResult.Errors.First().Message}");
        }

        // Get the parsed credential
        var credential = parseResult.Value;
        
        // Use PayloadJson property which contains the original JSON structure
        // rather than ToJson() which might restructure the data
        string jsonCredential = !string.IsNullOrEmpty(credential.PayloadJson) 
            ? credential.PayloadJson 
            : request.Credential; // Fallback to original request if PayloadJson is empty
            
        JsonDocument credentialJson;
        try
        {
            credentialJson = JsonDocument.Parse(jsonCredential);
        }
        catch (JsonException ex)
        {
            return Result.Fail<W3cValidationResponse>($"Invalid credential format after parsing: {ex.Message}");
        }

        var result = new W3cValidationResponse();
        try
        {
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
        catch (Exception ex)
        {
            return Result.Fail<W3cValidationResponse>($"Validation failed: {ex.Message}");
        }
    }

    private void ValidateRequiredField(JsonDocument credential, ValidationRule rule, W3cValidationResponse response)
    {
        var path = rule.Configuration;
        var pathParts = path.Split('.');
        
        // Try direct path traversal
        bool found = TryFindPath(credential.RootElement, pathParts);
        
        // If not found directly and path doesn't start with "vc", try through vc property
        if (!found && !pathParts[0].Equals("vc", StringComparison.OrdinalIgnoreCase))
        {
            if (credential.RootElement.TryGetProperty("vc", out var vcElement))
            {
                found = TryFindPath(vcElement, pathParts);
            }
        }
        
        if (!found)
        {
            response.Errors.Add(new W3cValidationError("Required", $"Required field '{path}' is missing"));
        }
    }

    private bool TryFindPath(JsonElement element, string[] pathParts)
    {
        var currentElement = element;
        
        foreach (var part in pathParts)
        {
            if (!currentElement.TryGetProperty(part, out var child))
            {
                return false;
            }
            currentElement = child;
        }
        
        return true;
    }

    private void ValidateFormat(JsonDocument credential, ValidationRule rule, W3cValidationResponse response)
    {
        var config = rule.Configuration.Split(':');
        if (config.Length != 2)
        {
            response.Errors.Add(new W3cValidationError("Format", "Invalid format rule configuration"));
            return;
        }

        var path = config[0];
        var format = config[1];
        var pathParts = path.Split('.');
        
        // Try to find the element using the same approach as in ValidateRequiredField
        JsonElement element;
        bool found = TryGetElement(credential.RootElement, pathParts, out element);
        
        if (!found)
        {
            response.Errors.Add(new W3cValidationError("Format", $"Field '{path}' not found"));
            return;
        }

        switch (format.ToUpper())
        {
            case "ISO8601":
                if (element.ValueKind != JsonValueKind.String || 
                    !DateTime.TryParse(element.GetString(), out _))
                {
                    response.Errors.Add(new W3cValidationError("Format", $"Field '{path}' is not a valid ISO8601 date"));
                }
                break;
            case "EMAIL":
                if (element.ValueKind != JsonValueKind.String || 
                    !IsValidEmail(element.GetString()))
                {
                    response.Errors.Add(new W3cValidationError("Format", $"Field '{path}' is not a valid email format"));
                }
                break;
            case "URL":
                if (element.ValueKind != JsonValueKind.String || 
                    !Uri.TryCreate(element.GetString(), UriKind.Absolute, out _))
                {
                    response.Errors.Add(new W3cValidationError("Format", $"Field '{path}' is not a valid URL format"));
                }
                break;
            case "DID":
                if (element.ValueKind != JsonValueKind.String || 
                    !element.GetString().StartsWith("did:"))
                {
                    response.Errors.Add(new W3cValidationError("Format", $"Field '{path}' is not a valid DID format"));
                }
                break;
            default:
                response.Errors.Add(new W3cValidationError("Format", $"Unsupported format '{format}'"));
                break;
        }
    }

    private void ValidateRange(JsonDocument credential, ValidationRule rule, W3cValidationResponse response)
    {
        var config = rule.Configuration.Split(':');
        if (config.Length != 2)
        {
            response.Errors.Add(new W3cValidationError("Range", "Invalid range rule configuration"));
            return;
        }

        var path = config[0];
        var range = config[1].Split('-');
        if (range.Length != 2)
        {
            response.Errors.Add(new W3cValidationError("Range", "Invalid range format"));
            return;
        }

        if (!double.TryParse(range[0], out var min) || !double.TryParse(range[1], out var max))
        {
            response.Errors.Add(new W3cValidationError("Range", "Invalid range values"));
            return;
        }

        var pathParts = path.Split('.');
        JsonElement element;
        bool found = TryGetElement(credential.RootElement, pathParts, out element);
        
        if (!found)
        {
            response.Errors.Add(new W3cValidationError("Range", $"Field '{path}' not found"));
            return;
        }

        double value;
        if (element.ValueKind == JsonValueKind.Number)
        {
            try
            {
                value = element.GetDouble();
            }
            catch (InvalidOperationException)
            {
                response.Errors.Add(new W3cValidationError("Range", $"Field '{path}' cannot be retrieved as a number"));
                return;
            }
        }
        else if (element.ValueKind == JsonValueKind.String)
        {
            var stringValue = element.GetString();
            if (!double.TryParse(stringValue, out value))
            {
                response.Errors.Add(new W3cValidationError("Range", $"Field '{path}' is a string but cannot be parsed as a number"));
                return;
            }
        }
        else
        {
            response.Errors.Add(new W3cValidationError("Range", $"Field '{path}' is neither a number nor a string"));
            return;
        }

        if (value < min || value > max)
        {
            response.Errors.Add(new W3cValidationError("Range", $"Field '{path}' value {value} is outside range {min}-{max}"));
        }
    }

    private void ValidateCustomRule(JsonDocument credential, ValidationRule rule, W3cValidationResponse response)
    {
        // This method could be expanded in the future for custom validation logic
        response.Errors.Add(new W3cValidationError("Custom", "Custom validation rules are not implemented yet"));
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // Use a simple regex for email validation
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return regex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }
    
    private bool TryGetElement(JsonElement root, string[] pathParts, out JsonElement result)
    {
        result = root;
        
        // Try direct path first
        bool directPathFound = true;
        var directElement = root;
        
        foreach (var part in pathParts)
        {
            if (!directElement.TryGetProperty(part, out directElement))
            {
                directPathFound = false;
                break;
            }
        }
        
        if (directPathFound)
        {
            result = directElement;
            return true;
        }
        
        // If direct path fails and first part isn't "vc", try through vc property
        if (!pathParts[0].Equals("vc", StringComparison.OrdinalIgnoreCase) && 
            root.TryGetProperty("vc", out var vcElement))
        {
            bool vcPathFound = true;
            var currentElement = vcElement;
            
            foreach (var part in pathParts)
            {
                if (!currentElement.TryGetProperty(part, out currentElement))
                {
                    vcPathFound = false;
                    break;
                }
            }
            
            if (vcPathFound)
            {
                result = currentElement;
                return true;
            }
        }
        
        return false;
    }
}