using System.Text;
using FluentResults;
using System.Text.Json;
using System.Text.Json.Serialization;
using Blocktrust.CredentialWorkflow.Core.Domain.Credential;
using Blocktrust.VerifiableCredential;
using Blocktrust.VerifiableCredential.Common;
using Blocktrust.VerifiableCredential.VC;

namespace Blocktrust.CredentialWorkflow.Core.Services;

public class CredentialParser
{
    public Result<Credential> ParseCredential(string credentialString)
    {
        if (string.IsNullOrEmpty(credentialString))
        {
            return Result.Fail("Credential string cannot be empty");
        }

        try
        {
            var jwtResult = JwtParser.Parse(credentialString);
            if (jwtResult.IsSuccess && jwtResult.Value.VerifiableCredentials.Any())
            {
                var baseCredential = jwtResult.Value.VerifiableCredentials.First();
                var credential = new Credential(baseCredential)
                {
                    CredentialContext = null,
                    Type = null,
                    CredentialSubjects = null
                };

                // Split JWT to get header and payload
                var parts = credentialString.Split('.');
                if (parts.Length >= 2)
                {
                    credential.HeaderJson = Encoding.UTF8.GetString(Base64Url.Decode(parts[0]));
                    credential.PayloadJson = Encoding.UTF8.GetString(Base64Url.Decode(parts[1]));
                }

                return Result.Ok(credential);
            }
        }
        catch
        {
            // Intentionally ignore JWT parsing errors to try JSON-LD parsing
        }

        try
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var baseCredential = JsonSerializer.Deserialize<VerifiableCredential.VerifiableCredential>(credentialString, jsonOptions);
            if (baseCredential == null)
            {
                return Result.Fail("Failed to parse credential as JSON-LD");
            }

            var credential = new Credential(baseCredential)
            {
                HeaderJson = null, // JSON-LD doesn't have JWT parts
                PayloadJson = credentialString,
                CredentialContext = null,
                Type = null,
                CredentialSubjects = null // Store the whole JSON-LD as payload
            };

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
}