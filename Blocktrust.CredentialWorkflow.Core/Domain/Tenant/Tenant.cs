﻿namespace Blocktrust.CredentialWorkflow.Core.Domain.Tenant;

public record Tenant
{
    public Guid TenantId { get; init; }

    public string Name { get; init; }

    public DateTime CreatedUtc { get; init; }
}