namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Action;

public class ActionResult
{
    public bool Success { get; set; }
    public string? OutputJson { get; set; }
    public string? ErrorMessage { get; set; }
}