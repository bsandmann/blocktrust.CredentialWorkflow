// namespace Blocktrust.CredentialWorkflow.Core.Commands.Tenant.GetIdentusAgents;
//
// using FluentResults;
// using MediatR;
// using Microsoft.EntityFrameworkCore;
// using Blocktrust.CredentialWorkflow.Core.Entities.Tenant;
//
// public class GetIdentusAgentsRequest : IRequest<Result<List<IdentusAgent>>>
// {
//     public GetIdentusAgentsRequest(Guid tenantId)
//     {
//         TenantId = tenantId;
//     }
//
//     public Guid TenantId { get; }
// }