using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NJsonSchema;

namespace Blocktrust.CredentialWorkflow.Core.Services
{
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

                var errors = schema.Validate(json);

                return errors.Select(error => $"{error.Path} - {error.Kind}");
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
}