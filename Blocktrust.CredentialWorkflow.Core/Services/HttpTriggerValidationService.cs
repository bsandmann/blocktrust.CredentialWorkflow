using System.Text.Json;
using Blocktrust.CredentialWorkflow.Core.Domain.Common;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Triggers;
using FluentResults;

namespace Blocktrust.CredentialWorkflow.Core.Services;

public class HttpTriggerValidationService : ITriggerValidationService
{
    public Result Validate(
        TriggerInputHttpRequest triggerInput,
        SimplifiedHttpContext httpContext)
    {
        // Validate HTTP method (case-insensitive).
        // triggerInput.Method is a string like "POST" or "GET";
        // httpContext.Method is an enum; call .ToString().
        if (!string.Equals(triggerInput.Method,
                httpContext.Method.ToString(),
                StringComparison.OrdinalIgnoreCase))
        {
            return Result.Fail($"HTTP method mismatch. Expected '{triggerInput.Method}', got '{httpContext.Method}'.");
        }

        // Attempt to parse the request body as a JSON dictionary of string->string (if present).
        // We'll store the result in bodyValues. If the body is empty or invalid, treat as empty dictionary.
        Dictionary<string, string> bodyValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrEmpty(httpContext.Body))
        {
            try
            {
                // Assumption: Body is a JSON object that can be deserialized as a dictionary of string->string
                bodyValues = JsonSerializer.Deserialize<Dictionary<string, string>>(httpContext.Body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                return Result.Fail($"Failed to parse body as JSON Dictionary<string,string>: {ex.Message}");
            }
        }

        // For every parameter definition in triggerInput.Parameters:
        // - Check if it is required
        // - If required, ensure we can find a value in either query or body
        // - If a value is found (whether required or not), validate AllowedValues (case-insensitive)
        //   and validate the type: String, Number, Boolean
        foreach (var kvp in triggerInput.Parameters)
        {
            string paramName = kvp.Key;           // The key (e.g. "userId")
            var definition = kvp.Value;           // The ParameterDefinition object
            bool isRequired = definition.Required;

            string? providedValue = TryGetValueCaseInsensitive(httpContext.QueryParameters, paramName);

            if (providedValue == null)
            {
                providedValue = TryGetValueCaseInsensitive(bodyValues, paramName);
            }

            // If required but not found at all, fail
            if (isRequired && providedValue == null)
            {
                return Result.Fail($"Required parameter '{paramName}' was not provided.");
            }

            // If the parameter was not provided and it's not required, skip further checks.
            if (providedValue == null)
                continue;

            // if AllowedValues are defined, check ignoring case.
            if (definition.AllowedValues != null && definition.AllowedValues.Length > 0)
            {
                bool inAllowedValues = definition.AllowedValues
                    .Any(allowed =>
                        string.Equals(allowed, providedValue, StringComparison.OrdinalIgnoreCase));

                if (!inAllowedValues)
                {
                    return Result.Fail(
                        $"Parameter '{paramName}' has value '{providedValue}' which is not in the allowed set [{string.Join(", ", definition.AllowedValues)}].");
                }
            }

            // 5. Validate parameter type
            //    If type is Number, try to parse as double.
            //    If type is Boolean, try bool parse.
            //    If type is String, there's no extra parse check beyond the AllowedValues check above.
            switch (definition.Type)
            {
                case ParameterType.String:
                    // No further action needed beyond allowed values check.
                    break;

                case ParameterType.Number:
                {
                    // Generous parsing as double
                    if (!double.TryParse(providedValue, out _))
                    {
                        return Result.Fail($"Parameter '{paramName}' was expected to be a Number but value '{providedValue}' is invalid.");
                    }
                    break;
                }

                case ParameterType.Boolean:
                {
                    // Generous parsing as bool
                    if (!bool.TryParse(providedValue, out _))
                    {
                        return Result.Fail($"Parameter '{paramName}' was expected to be a Boolean but value '{providedValue}' is invalid.");
                    }
                    break;
                }

                default:
                    return Result.Fail($"Parameter '{paramName}' has unknown/unsupported type '{definition.Type}'.");
            }
        }

        return Result.Ok();
    }

    /// <summary>
    /// Attempts to retrieve a value from the given dictionary by matching the key in a case-insensitive way.
    /// Returns the value if found, otherwise null.
    /// </summary>
    private static string? TryGetValueCaseInsensitive(IDictionary<string, string> dictionary, string key)
    {
        foreach (var kvp in dictionary)
        {
            if (string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase))
            {
                return kvp.Value;
            }
        }
        return null;
    }
}