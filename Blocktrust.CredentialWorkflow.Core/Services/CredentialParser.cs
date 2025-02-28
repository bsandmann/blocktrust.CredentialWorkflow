using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Blocktrust.CredentialWorkflow.Core.Domain.Credential;
using Blocktrust.CredentialWorkflow.Core.Prism;
using Blocktrust.VerifiableCredential;
using Blocktrust.VerifiableCredential.Common;
using FluentResults;

namespace Blocktrust.CredentialWorkflow.Core.Services;

using VerifiableCredential = VerifiableCredential.VerifiableCredential;

public class CredentialParser
{
    public Result<Credential> ParseCredential(string credentialString)
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

        // Try decoding as base64/base64url if the string looks like base64
        if (LooksLikeBase64(credentialString))
        {
            try
            {
                string decodedString;
                
                // First try with base64url
                try
                {
                    decodedString = PrismEncoding.Base64ToString(credentialString);
                    
                    // Check if the decoded string is not empty and is usable
                    if (!string.IsNullOrWhiteSpace(decodedString))
                    {
                        // Recursively try to parse the decoded string
                        return ParseCredential(decodedString);
                    }
                }
                catch (Exception ex)
                {
                    // Try with standard base64 if base64url fails
                    try 
                    {
                        var paddedInput = credentialString;
                        // Add padding if needed
                        if (credentialString.Length % 4 != 0)
                        {
                            paddedInput = credentialString.PadRight(
                                credentialString.Length + (4 - credentialString.Length % 4) % 4, '=');
                        }
                        
                        decodedString = Encoding.UTF8.GetString(Convert.FromBase64String(paddedInput));
                        
                        // Check if the decoded string is not empty and is usable
                        if (!string.IsNullOrWhiteSpace(decodedString))
                        {
                            // Recursively try to parse the decoded string
                            return ParseCredential(decodedString);
                        }
                    }
                    catch (Exception innerEx)
                    {
                        errors.Add($"Base64 decoding failed: {innerEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Base64 decoding failed: {ex.Message}");
            }
        }
        else
        {
            errors.Add("Input does not appear to be base64 encoded");
        }

        // If parsing failed but it still looks like JSON, create a simplified Credential
        // This handles the case of invalid date formats in otherwise valid JSON
        if (IsLikelyJson(credentialString) && IsSyntacticallyValidJson(credentialString))
        {
            var credential = new Credential
            {
                HeaderJson = null,
                PayloadJson = credentialString,
                CredentialContext = null,
                Type = null,
                CredentialSubjects = null
            };
            
            return Result.Ok(credential);
        }

        // If all parsing attempts failed, return aggregate error
        return Result.Fail($"Failed to parse credential. Errors: {string.Join(", ", errors)}");
    }

    private bool IsLikelyJson(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;
            
        var trimmed = input.TrimStart();
        return trimmed.StartsWith("{") || trimmed.StartsWith("[");
    }
    
    private bool IsSyntacticallyValidJson(string input)
    {
        try
        {
            JsonDocument.Parse(input);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    private bool LooksLikeJwt(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;
            
        // Simple JWT format check: three base64url parts separated by dots
        var parts = input.Split('.');
        return parts.Length >= 2 && parts.All(p => p.Length > 0);
    }

    private bool LooksLikeBase64(string input)
    {
        // Return false for empty input
        if (string.IsNullOrEmpty(input))
            return false;
            
        // Skip if it looks like JSON (starts with { or [)
        if (IsLikelyJson(input))
            return false;
        
        // Skip if it's too short to be meaningful base64
        if (input.Length < 8)
            return false;
            
        // Check if the string contains only valid base64 characters
        bool hasInvalidChar = false;
        foreach (char c in input)
        {
            if (!((c >= 'A' && c <= 'Z') || 
                 (c >= 'a' && c <= 'z') || 
                 (c >= '0' && c <= '9') || 
                 c == '+' || c == '/' || c == '=' || 
                 c == '-' || c == '_')) // Include URL-safe characters
            {
                hasInvalidChar = true;
                break;
            }
        }
        
        return !hasInvalidChar;
    }

    private Result<Credential> TryParseAsJson(string input)
    {
        try
        {
            // First check if it's valid JSON syntax
            if (!IsSyntacticallyValidJson(input))
            {
                return Result.Fail("Not a valid JSON format");
            }
            
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            try
            {
                var baseCredential = JsonSerializer.Deserialize<VerifiableCredential>(input, jsonOptions);
                if (baseCredential == null)
                {
                    return Result.Fail("Failed to parse credential as JSON-LD");
                }

                var credential = new Credential(baseCredential)
                {
                    HeaderJson = null,
                    PayloadJson = input,
                    CredentialContext = null,
                    Type = null,
                    CredentialSubjects = null
                };

                return Result.Ok(credential);
            }
            catch (JsonException)
            {
                // If deserialization to VerifiableCredential fails, we'll return a failure
                // The main ParseCredential method will create a simplified Credential if needed
                return Result.Fail("Failed to deserialize to VerifiableCredential");
            }
        }
        catch (Exception ex)
        {
            return Result.Fail($"JSON parsing error: {ex.Message}");
        }
    }

    private Result<Credential> TryParseAsJwt(string input)
    {
        try
        {
            var jwtResult = JwtParser.Parse(input);
            if (!jwtResult.IsSuccess || !jwtResult.Value?.VerifiableCredentials?.Any() == true)
            {
                return Result.Fail("Not a valid JWT credential");
            }

            var baseCredential = jwtResult.Value.VerifiableCredentials.First();
            if (baseCredential == null)
            {
                return Result.Fail("JWT credential contains no verifiable credential");
            }

            var credential = new Credential(baseCredential)
            {
                CredentialContext = null,
                Type = null,
                CredentialSubjects = null
            };

            var parts = input.Split('.');
            if (parts.Length >= 2)
            {
                try
                {
                    credential.HeaderJson = Encoding.UTF8.GetString(Base64Url.Decode(parts[0]));
                    credential.PayloadJson = Encoding.UTF8.GetString(Base64Url.Decode(parts[1]));
                }
                catch (Exception ex)
                {
                    return Result.Fail($"Failed to decode JWT parts: {ex.Message}");
                }
            }

            return Result.Ok(credential);
        }
        catch (Exception ex)
        {
            return Result.Fail($"JWT parsing error: {ex.Message}");
        }
    }
}