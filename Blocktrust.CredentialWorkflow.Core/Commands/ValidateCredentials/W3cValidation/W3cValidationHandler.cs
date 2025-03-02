using System.Text.Json;
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
                        var requiredResult = ValidationUtility.ValidateRequiredField(credentialJson, rule);
                        if (!requiredResult.IsValid)
                        {
                            result.Errors.Add(new W3cValidationError("Required", requiredResult.ErrorMessage));
                        }
                        break;
                    case "format":
                        var formatResult = ValidationUtility.ValidateFormat(credentialJson, rule);
                        if (!formatResult.IsValid)
                        {
                            result.Errors.Add(new W3cValidationError("Format", formatResult.ErrorMessage));
                        }
                        break;
                    case "range":
                        var rangeResult = ValidationUtility.ValidateRange(credentialJson, rule);
                        if (!rangeResult.IsValid)
                        {
                            result.Errors.Add(new W3cValidationError("Range", rangeResult.ErrorMessage));
                        }
                        break;
                    case "value":
                        var valueResult = ValidationUtility.ValidateValue(credentialJson, rule);
                        if (!valueResult.IsValid)
                        {
                            result.Errors.Add(new W3cValidationError("Value", valueResult.ErrorMessage));
                        }
                        break;
                    case "valuearray":
                        var valueArrayResult = ValidationUtility.ValidateValueArray(credentialJson, rule);
                        if (!valueArrayResult.IsValid)
                        {
                            result.Errors.Add(new W3cValidationError("ValueArray", valueArrayResult.ErrorMessage));
                        }
                        break;
                    case "custom":
                        var customResult = ValidationUtility.ValidateCustomRule(credentialJson, rule);
                        if (!customResult.IsValid)
                        {
                            result.Errors.Add(new W3cValidationError("Custom", customResult.ErrorMessage));
                        }
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
}