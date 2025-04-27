using Microsoft.Extensions.Logging;
using NJsonSchema;
using Newtonsoft.Json.Linq;

namespace Blocktrust.CredentialWorkflow.Core.Services;

public class SchemaValidationService : ISchemaValidationService
{
    private readonly ILogger<SchemaValidationService> _logger;
    private readonly string _schemaDirectory;

    public SchemaValidationService(ILogger<SchemaValidationService> logger)
    {
        _logger = logger;
        _schemaDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Domain/JsonSchema");
    }

    public async Task<IEnumerable<string>> ValidateJsonAgainstSchema(string json, string schemaName)
    {
        try
        {
            string schemaPath = Path.Combine(_schemaDirectory, $"{schemaName}.json");
            string schemaJson = await File.ReadAllTextAsync(schemaPath);
            var schema = await JsonSchema.FromJsonAsync(schemaJson);
            
            // Parse the JSON to validate with Newtonsoft.Json
            var jsonObject = JToken.Parse(json);
            
            // Validate using NJsonSchema
            var errors = schema.Validate(jsonObject);
            
            // Filter out any errors related to "additionalProperties" validation
            var filteredErrors = errors.Where(error => !error.Kind.ToString().Contains("AdditionalProperties"));

            _logger.LogInformation("Schema validation completed. Total errors: {TotalErrors}, Filtered errors: {FilteredErrors}", 
                errors.Count, filteredErrors.Count());
            
            return filteredErrors.Select(error => $"{error.Path} - {error.Kind}");
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Error reading schema file: {SchemaName}", schemaName);
            throw new InvalidOperationException($"Unable to load schema: {schemaName}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating JSON against schema: {SchemaName}", schemaName);
            throw new InvalidOperationException($"Error during schema validation: {schemaName}", ex);
        }
    }
}