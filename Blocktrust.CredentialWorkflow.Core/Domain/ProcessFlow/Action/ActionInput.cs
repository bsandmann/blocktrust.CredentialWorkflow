namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Action;

using System.Text.Json.Serialization;

public class ActionInput
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
}