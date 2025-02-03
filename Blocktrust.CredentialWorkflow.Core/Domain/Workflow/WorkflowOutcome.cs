using System.Text.Json.Serialization;
using Blocktrust.CredentialWorkflow.Core.Domain.Common;
using Blocktrust.CredentialWorkflow.Core.Domain.Enums;

namespace Blocktrust.CredentialWorkflow.Core.Domain.Workflow;

using ProcessFlow.Actions;

/// <summary>
/// Each workflow usually runs multiple times. Each of those runs is a Workflow-Outcome
/// </summary>
public class WorkflowOutcome
{
    [JsonPropertyName("workflowOutcomeId")]
    public Guid WorkflowOutcomeId { get; init; }

    [JsonPropertyName("workflowId")] public Guid WorkflowId { get; init; }

    [JsonPropertyName("outcomeState")] public EWorkflowOutcomeState WorkflowOutcomeState { get; set; }
    public DateTime? StartedUtc { get; set; }

    public DateTime? EndedUtc { get; set; }

    [JsonPropertyName("errorJson")] public string? ErrorJson { get; set; } = string.Empty;

    [JsonPropertyName("actionOutcomeJson")]
    public string? ActionOutcomesJson { get; set; } = string.Empty;

    public List<ActionOutcome> InMemoryActionOutcomes { get; set; } = new();

    public string? ExecutionContext { get; set; }
}