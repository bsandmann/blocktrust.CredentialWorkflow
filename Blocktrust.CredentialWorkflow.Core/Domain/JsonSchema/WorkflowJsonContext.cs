using System.Text.Json.Serialization;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Outgoing;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Triggers;
using Action = Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Action;

namespace Blocktrust.CredentialWorkflow.Core.Domain.JsonSchema;

using Common;
using ProcessFlow = ProcessFlow.ProcessFlow;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ProcessFlow))]
[JsonSerializable(typeof(Trigger))]
[JsonSerializable(typeof(TriggerInput))]
[JsonSerializable(typeof(TriggerInputHttpRequest))]
[JsonSerializable(typeof(TriggerInputRecurringTimer))]
[JsonSerializable(typeof(TriggerInputOnDemand))]
[JsonSerializable(typeof(Action))]
[JsonSerializable(typeof(ActionInput))]
[JsonSerializable(typeof(MessageFieldValue))]
[JsonSerializable(typeof(DIDCommAction))]
[JsonSerializable(typeof(HttpAction))]
[JsonSerializable(typeof(EmailAction))]
public partial class WorkflowJsonContext : JsonSerializerContext
{
}