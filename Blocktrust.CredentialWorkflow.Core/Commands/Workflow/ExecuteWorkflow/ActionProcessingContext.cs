using Blocktrust.CredentialWorkflow.Core.Domain.Common;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;
using System.Threading;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow;

using ExecutionContext = Domain.Common.ExecutionContext;

public class ActionProcessingContext
{
    public ExecutionContext ExecutionContext { get; }
    public List<ActionOutcome> ActionOutcomes { get; }
    public Domain.Workflow.Workflow Workflow { get; }
    public CancellationToken CancellationToken { get; }

    public ActionProcessingContext(
        ExecutionContext executionContext,
        List<ActionOutcome> actionOutcomes,
        Domain.Workflow.Workflow workflow,
        CancellationToken cancellationToken)
    {
        ExecutionContext = executionContext;
        ActionOutcomes = actionOutcomes;
        Workflow = workflow;
        CancellationToken = cancellationToken;
    }
}