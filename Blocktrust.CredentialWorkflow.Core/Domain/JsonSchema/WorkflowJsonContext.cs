using System.Text.Json.Serialization;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Input;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Triggers;
using Action = Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Action;

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
[JsonSerializable(typeof(ActionInputDIDCommTrustPing))]
[JsonSerializable(typeof(ActionInputDIDCommMessage))]
[JsonSerializable(typeof(MessageFieldValue))]
// Add other ActionInput derived types here
public partial class WorkflowJsonContext : JsonSerializerContext
{
}