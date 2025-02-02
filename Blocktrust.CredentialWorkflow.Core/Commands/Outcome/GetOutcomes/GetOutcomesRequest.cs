using Blocktrust.CredentialWorkflow.Core.Domain.Common;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Outcome.GetOutcomes;

using Domain.ProcessFlow.Actions;
using FluentResults;
using MediatR;

public class GetOutcomesRequest : IRequest<Result<List<ActionOutcome>>>
{
    public GetOutcomesRequest(Guid workflowId)
    {
        WorkflowId = workflowId;
    }

    public Guid WorkflowId { get; }
}