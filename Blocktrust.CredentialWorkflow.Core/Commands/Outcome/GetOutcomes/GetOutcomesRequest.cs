namespace Blocktrust.CredentialWorkflow.Core.Commands.Outcome.GetOutcomes;

using Blocktrust.CredentialWorkflow.Core.Domain.Outcome;
using FluentResults;
using MediatR;

public class GetOutcomesRequest : IRequest<Result<List<Outcome>>>
{
    public GetOutcomesRequest(Guid workflowId)
    {
        WorkflowId = workflowId;
    }

    public Guid WorkflowId { get; }
}