using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Validation;
using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.ValidateCredentials.W3cValidation;

using Prism;
using VerifiableCredential;

public class W3cValidationHandler : IRequestHandler<W3cValidationRequest, Result<W3cValidationResponse>>
{
    public async Task<Result<W3cValidationResponse>> Handle(W3cValidationRequest request, CancellationToken cancellationToken)
    {
        // First, try to parse the credential
        var parseResult = ParseCredential(request.Credential);
        if (parseResult.IsFailed)
        {
            return Result.Fail(parseResult.Errors);
        }

        var verifiableCredential = parseResult.Value;
        string jsonCredential = verifiableCredential.ToJson();

        // Now, parse the JSON string into JsonDocument
        JsonDocument credentialJson;
        try
        {
            credentialJson = JsonDocument.Parse(jsonCredential);
        }
        catch (JsonException ex)
        {
            return Result.Fail($"Invalid credential format after parsing: {ex.Message}");
        }

        // Proceed with validation
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
            return Result.Fail($"Validation failed: {ex.Message}");
        }
    }

    // ParseCredential method to handle different credential formats
    private Result<VerifiableCredential> ParseCredential(string credentialString)
    {
        if (string.IsNullOrEmpty(credentialString))
        {
            return Result.Fail("Credential string cannot be empty");
        }

        // Try each format, collecting errors for better debugging
        var errors = new List<string>();

        // Try as JWT first (most common format)
        var jwtResult = TryParseAsJwt(credentialString);
        if (jwtResult.IsSuccess)
        {
            return jwtResult;
        }
        errors.Add(jwtResult.Errors.First().Message);

        // Try direct JSON/JSON-LD
        var jsonResult = TryParseAsJson(credentialString);
        if (jsonResult.IsSuccess)
        {
            return jsonResult;
        }
        errors.Add(jsonResult.Errors.First().Message);

        // Try decoding as base64/base64url
        try
        {
            string decodedString;
            try
            {
                // Try base64url first
                decodedString = PrismEncoding.Base64ToString(credentialString);
            }
            catch
            {
                // Fall back to regular base64
                decodedString = Encoding.UTF8.GetString(Convert.FromBase64String(
                    credentialString.PadRight(credentialString.Length + (4 - credentialString.Length % 4) % 4, '=')));
            }

            // Recursively try to parse the decoded string
            return ParseCredential(decodedString);
        }
        catch (Exception ex)
        {
            errors.Add($"Base64 decoding failed: {ex.Message}");
        }

        // If all parsing attempts failed, return aggregate error
        return Result.Fail($"Failed to parse credential. Errors: {string.Join(", ", errors)}");
    }

    // Try to parse the input as JSON
    private Result<VerifiableCredential> TryParseAsJson(string input)
    {
        try
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            var credential = JsonSerializer.Deserialize<VerifiableCredential>(input, jsonOptions);
            if (credential != null)
            {
                return Result.Ok(credential);
            }
            else
            {
                return Result.Fail("Deserialization returned null");
            }
        }
        catch (JsonException ex)
        {
            return Result.Fail($"JSON parsing failed: {ex.Message}");
        }
    }

    // Try to parse the input as JWT
    private Result<VerifiableCredential> TryParseAsJwt(string input)
    {
        var jwtResult = JwtParser.Parse(input);
        if (!jwtResult.IsSuccess || !jwtResult.Value?.VerifiableCredentials?.Any() == true)
        {
            return Result.Fail("Not a valid JWT credential");
        }

        var baseCredential = jwtResult.Value.VerifiableCredentials.First();
        return Result.Ok(baseCredential);
    }

    // Existing validation methods (unchanged)
    private void ValidateRequiredField(JsonDocument credential, ValidationRule rule, W3cValidationResponse response)
    {
        var path = rule.Configuration;
        var pathParts = path.Split('.');
        var element = credential.RootElement;

        if (element.ValueKind == JsonValueKind.Null)
        {
            response.Errors.Add(new W3cValidationError("Required", $"Required field '{path}' is null"));
        }

        foreach (var part in pathParts)
        {
            if (!element.TryGetProperty(part, out var child))
            {
                response.Errors.Add(new W3cValidationError("Required", $"Required field '{path}' is missing"));
                return;
            }
            element = child;
        }
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
        var element = credential.RootElement;

        foreach (var part in pathParts)
        {
            if (!element.TryGetProperty(part, out var child))
            {
                response.Errors.Add(new W3cValidationError("Format", $"Field '{path}' not found"));
                return;
            }
            element = child;
        }

        switch (format.ToUpper())
        {
            case "ISO8601":
                if (!DateTime.TryParse(element.GetString(), out _))
                {
                    response.Errors.Add(new W3cValidationError("Format", $"Field '{path}' is not a valid ISO8601 date"));
                }
                break;
        }
    }

    private void ValidateRange(JsonDocument credential, ValidationRule rule, W3cValidationResponse result)
    {
        var config = rule.Configuration.Split(':');
        if (config.Length != 2)
        {
            result.Errors.Add(new W3cValidationError("Range", "Invalid range rule configuration"));
            return;
        }

        var path = config[0];
        var range = config[1].Split('-');
        if (range.Length != 2)
        {
            result.Errors.Add(new W3cValidationError("Range", "Invalid range format"));
            return;
        }

        if (!double.TryParse(range[0], out var min) || !double.TryParse(range[1], out var max))
        {
            result.Errors.Add(new W3cValidationError("Range", "Invalid range values"));
            return;
        }

        var pathParts = path.Split('.');
        var element = credential.RootElement;
        foreach (var part in pathParts)
        {
            if (!element.TryGetProperty(part, out var child))
            {
                result.Errors.Add(new W3cValidationError("Range", $"Field '{path}' not found"));
                return;
            }
            element = child;
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
                result.Errors.Add(new W3cValidationError("Range", $"Field '{path}' cannot be retrieved as a number"));
                return;
            }
        }
        else if (element.ValueKind == JsonValueKind.String)
        {
            var stringValue = element.GetString();
            if (!double.TryParse(stringValue, out value))
            {
                result.Errors.Add(new W3cValidationError("Range", $"Field '{path}' is a string but cannot be parsed as a number"));
                return;
            }
        }
        else
        {
            result.Errors.Add(new W3cValidationError("Range", $"Field '{path}' is neither a number nor a string"));
            return;
        }

        if (value < min || value > max)
        {
            result.Errors.Add(new W3cValidationError("Range", $"Field '{path}' value {value} is outside range {min}-{max}"));
        }
    }

    private void ValidateCustomRule(JsonDocument credential, ValidationRule rule, W3cValidationResponse response)
    {
        response.Errors.Add(new W3cValidationError("Custom", "Custom validation rules are not implemented"));
    }
}