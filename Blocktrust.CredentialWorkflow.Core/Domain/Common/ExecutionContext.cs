using System.Collections.ObjectModel;
using System.Text.Json;

namespace Blocktrust.CredentialWorkflow.Core.Domain.Common;

/// <summary>
/// Represents an execution context that combines parameters from the HTTP query string and request body.
/// Query parameters have precedence over body parameters when keys conflict.
/// </summary>
public class ExecutionContext
{
    /// <summary>
    /// Gets the combined input parameters from the query string and request body.
    /// </summary>
    public IReadOnlyDictionary<string, string>? InputContext { get; }

    public Guid TenantId { get; set; }

    // Private constructor forces usage of the static factory method.
    public ExecutionContext(Guid tenantId, IReadOnlyDictionary<string, string>? inputContext = null)
    {
        TenantId = tenantId;
        InputContext = inputContext;
    }

    public static ExecutionContext FromSimplifiedHttpContext(Guid tenantId, string context)
    {
        if (string.IsNullOrWhiteSpace(context))
        {
            throw new ArgumentException("Context cannot be null or empty.", nameof(context));
        }

        SimplifiedHttpContext simplifiedHttpContext;
        try
        {
            simplifiedHttpContext = SimplifiedHttpContext.FromJson(context);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("Invalid JSON provided for SimplifiedHttpContext.", nameof(context), ex);
        }

        if (simplifiedHttpContext == null)
        {
            throw new InvalidOperationException("SimplifiedHttpContext could not be deserialized.");
        }

        // Start with query parameters. These have precedence over body parameters.
        var mergedParameters = new Dictionary<string, string>(simplifiedHttpContext.QueryParameters.Select((x) => new KeyValuePair<string, string>(x.Key.ToLowerInvariant().Trim(), x.Value)));

        // If a body is provided, merge its parameters.
        if (!string.IsNullOrWhiteSpace(simplifiedHttpContext.Body))
        {
            Dictionary<string, string>? bodyParameters = null;
            try
            {
                bodyParameters = JsonSerializer.Deserialize<Dictionary<string, string>>(simplifiedHttpContext.Body);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Failed to parse body JSON.", ex);
            }

            if (bodyParameters != null)
            {
                // Use TryAdd so that existing query parameters (already in the dictionary) are not overwritten.
                foreach (var bodyParam in bodyParameters)
                {
                    mergedParameters.TryAdd(bodyParam.Key.ToLowerInvariant().Trim(), bodyParam.Value);
                }
            }
        }

        return new ExecutionContext(tenantId, new ReadOnlyDictionary<string, string>(mergedParameters));
    }

    public static ExecutionContext FromForm(Guid tenantId, string context)
    {
        if (string.IsNullOrWhiteSpace(context))
        {
            throw new ArgumentException("Context cannot be null or empty.", nameof(context));
        }

        try
        {
            using var jsonDoc = JsonDocument.Parse(context);
            var root = jsonDoc.RootElement;
            if (root.TryGetProperty("Data", out var dataElement) && dataElement.ValueKind == JsonValueKind.Object)
            {
                var parameters = new Dictionary<string, string>();
                foreach (var property in dataElement.EnumerateObject())
                {
                    string key = property.Name.ToLowerInvariant().Trim();
                    if (property.Value.ValueKind == JsonValueKind.String)
                    {
                        string value = property.Value.GetString();
                        parameters[key] = value;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Non-string value for key '{property.Name}' in FormSubmission data.");
                    }
                }
                return new ExecutionContext(tenantId, new ReadOnlyDictionary<string, string>(parameters));
            }
            else
            {
                throw new InvalidOperationException("FormSubmission data is missing or invalid.");
            }
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("Invalid JSON provided for FormSubmission.", nameof(context), ex);
        }
    }
}