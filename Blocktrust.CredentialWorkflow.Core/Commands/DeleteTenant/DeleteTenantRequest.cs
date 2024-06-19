﻿namespace Blocktrust.CredentialWorkflow.Core.Commands.DeleteTenant;

using FluentResults;
using MediatR;

public class DeleteTenantRequest : IRequest<Result>
{
    public DeleteTenantRequest(Guid tenantId)
    {
        TenantId = tenantId;
    }
    
    public Guid TenantId { get; }
}