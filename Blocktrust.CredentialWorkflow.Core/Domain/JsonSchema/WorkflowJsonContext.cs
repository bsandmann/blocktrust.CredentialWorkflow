using System.Text.Json.Serialization;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Action;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Trigger;
using Action = Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Action.Action;

namespace Blocktrust.CredentialWorkflow.Core.Domain.JsonSchema;

using ProcessFlow = ProcessFlow.ProcessFlow;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ProcessFlow))]
[JsonSerializable(typeof(Trigger))]
[JsonSerializable(typeof(TriggerInput))]
[JsonSerializable(typeof(TriggerInputIncomingRequest))]
[JsonSerializable(typeof(TriggerInputRecurringTimer))]
[JsonSerializable(typeof(TriggerInputOnDemand))]
[JsonSerializable(typeof(Action))]
[JsonSerializable(typeof(ActionInput))]
// Add other ActionInput derived types here
public partial class WorkflowJsonContext : JsonSerializerContext
{
}