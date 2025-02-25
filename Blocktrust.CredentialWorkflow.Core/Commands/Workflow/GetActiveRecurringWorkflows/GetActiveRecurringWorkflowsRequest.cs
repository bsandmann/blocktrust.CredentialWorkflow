using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.GetActiveRecurringWorkflows;

using Domain.Workflow;

public class GetActiveRecurringWorkflowsRequest : IRequest<Result<List<ActiveRecurringWorkflow>>>
{
}