namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.CreateWorkflow;

using Domain.ProcessFlow;
using Domain.Workflow;
using FluentResults;
using MediatR;

public class CreateWorkflowRequest : IRequest<Result<Workflow>>
{
    /// <summary>
    /// Creates a new empty workflow for the tenant
    /// </summary>
    /// <param name="tenantId"></param>
    public CreateWorkflowRequest(Guid tenantId)
    {
        TenantId = tenantId;
    }
    public CreateWorkflowRequest(Guid tenantId, string name, ProcessFlow processFlow)
    {
        TenantId = tenantId;
        Name = name;
        ProcessFlow = processFlow;
    }

    public Guid TenantId { get; }
    public string? Name { get; set; }
    public ProcessFlow? ProcessFlow { get; set; }
}