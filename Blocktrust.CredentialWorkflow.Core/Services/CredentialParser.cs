using FluentResults;
using System.Text.Json;
using Blocktrust.VerifiableCredential;
using Blocktrust.VerifiableCredential.VC;

namespace Blocktrust.CredentialWorkflow.Core.Services;

public class CredentialParser
{
    /// <summary>
    /// Parses a credential string which can be either a JWT or a JSON-LD Verifiable Credential
    /// </summary>
    /// <param name="credentialString">The credential string to parse</param>
    /// <returns>A Result containing either the parsed VerifiableCredential or an error</returns>
    public Result<VerifiableCredential.VerifiableCredential> ParseCredential(string credentialString)
    {
        if (string.IsNullOrEmpty(credentialString))
        {
            return Result.Fail("Credential string cannot be empty");
        }

        // First attempt to parse as JWT without throwing exception
        try
        {
            var jwtResult = JwtParser.Parse(credentialString);
            if (jwtResult.IsSuccess && jwtResult.Value.VerifiableCredentials.Any())
            {
                return Result.Ok(jwtResult.Value.VerifiableCredentials.First());
            }
        }
        catch
        {
            // Intentionally ignore JWT parsing errors to try JSON-LD parsing
        }

        // If JWT parsing failed or returned no credentials, try JSON-LD
        try
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            var credential = JsonSerializer.Deserialize<VerifiableCredential.VerifiableCredential>(credentialString, jsonOptions);
            if (credential == null)
            {
                return Result.Fail("Failed to parse credential as JSON-LD");
            }

            // Set the data model type
            credential.DataModelType = DataModelTypeEvaluator.Evaluate(credential);
            return Result.Ok(credential);
        }
        catch (JsonException ex)
        {
            return Result.Fail($"Failed to parse credential as JSON-LD: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result.Fail($"Unexpected error parsing credential: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks if a string appears to be a JWT
    /// </summary>
    /// <param name="input">The string to check</param>
    /// <returns>True if the string appears to be a JWT</returns>
    private bool IsJwt(string input)
    {
        // Basic JWT format check - three base64url segments separated by dots
        var segments = input.Split('.');
        return segments.Length >= 2 && segments.Length <= 3 
               && segments.All(s => !string.IsNullOrWhiteSpace(s))
               && segments.All(s => IsBase64UrlEncoded(s));
    }

    /// <summary>
    /// Checks if a string is base64url encoded
    /// </summary>
    /// <param name="input">The string to check</param>
    /// <returns>True if the string is base64url encoded</returns>
    private bool IsBase64UrlEncoded(string input)
    {
        try
        {
            // Replace URL-safe characters
            var base64 = input
                .Replace('-', '+')
                .Replace('_', '/');

            // Add padding if needed
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }

            Convert.FromBase64String(base64);
            return true;
        }
        catch
        {
            return false;
        }
    }
}