namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.GetWorkflowSummaries;

using Blocktrust.CredentialWorkflow.Core.Domain.Workflow;
using FluentResults;
using MediatR;

public class GetWorkflowSummariesRequest : IRequest<Result<List<WorkflowSummary>>>
{
    public GetWorkflowSummariesRequest(Guid tenantEntityId)
    {
        TenantEntityId = tenantEntityId;
    }

    public Guid TenantEntityId { get; }
}