namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.CreateWorkflow;

using FluentResults;
using MediatR;

public class CreateWorkflowRequest : IRequest<Result<Guid>>
{
    /// <summary>
    /// Creates a new empty workflow for the tenant
    /// </summary>
    /// <param name="tenantId"></param>
    public CreateWorkflowRequest(Guid tenantId)
    {
        TenantId = tenantId;
    }

    public Guid TenantId { get; }
}