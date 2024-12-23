using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Blocktrust.CredentialWorkflow.Core.Domain.Credential;
using Blocktrust.CredentialWorkflow.Core.Prism;
using Blocktrust.VerifiableCredential;
using Blocktrust.VerifiableCredential.Common;
using FluentResults;

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

    private Result<Credential> TryParseAsJson(string input)
    {
        try
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

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
            return Result.Fail("Not a valid JSON format");
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