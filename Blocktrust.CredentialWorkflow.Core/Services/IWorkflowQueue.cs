namespace Blocktrust.CredentialWorkflow.Core.Services;

public interface IWorkflowQueue
{
    Task EnqueueAsync(Guid outcomeId, CancellationToken cancellationToken = default);
}