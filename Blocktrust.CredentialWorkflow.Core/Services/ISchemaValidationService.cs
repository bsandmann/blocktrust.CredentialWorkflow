using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blocktrust.CredentialWorkflow.Core.Services
{
    public interface ISchemaValidationService
    {
        Task<IEnumerable<string>> ValidateJsonAgainstSchema(string json, string schemaName);
    }
}