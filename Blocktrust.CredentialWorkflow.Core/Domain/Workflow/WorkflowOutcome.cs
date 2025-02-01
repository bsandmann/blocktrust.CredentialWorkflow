using System.Text.Json.Serialization;
using Blocktrust.CredentialWorkflow.Core.Domain.Common;
using Blocktrust.CredentialWorkflow.Core.Domain.Enums;

namespace Blocktrust.CredentialWorkflow.Core.Domain.Workflow;

public class WorkflowOutcome
{
    [JsonPropertyName("workflowId")]

    public Guid WorkflowId { get; init; }
    
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("outcome")]
    public EOutcome Outcome { get; set; }
    
    [JsonPropertyName("message")]
    public string? Message { get; set; } = string.Empty;
    
    [JsonPropertyName("output")]
    public string? Output { get; set; } = string.Empty;
    
    [JsonPropertyName("updatedUtc")]

    public required DateTime UpdatedUtc { get; set; }
    
    [JsonPropertyName("workflowState")]

    public EWorkflowState WorkflowState { get; set; }
    
    [JsonPropertyName("lastOutcome")]

    public ActionOutcome? LastOutcome { get; set; }

    public bool IsRunable { get; set; }
    
}

public enum EOutcome
{
    Success,
    Failure
}