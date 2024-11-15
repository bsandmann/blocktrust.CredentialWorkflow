using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Action;

namespace Blocktrust.CredentialWorkflow.Core.Domain.Handlers;

using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow;
using FluentResults;

public interface IActionHandler
{
    Task<Result<ActionResult>> ExecuteAsync(
        ActionInput input,
        WorkflowContext context,
        CancellationToken cancellationToken);
}