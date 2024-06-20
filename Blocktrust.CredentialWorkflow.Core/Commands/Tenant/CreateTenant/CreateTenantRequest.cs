namespace Blocktrust.CredentialWorkflow.Core.Commands.Tenant.CreateTenant;

using FluentResults;
using MediatR;

public class CreateTenantRequest : IRequest<Result<Guid>>
{
    public CreateTenantRequest(string name)
    {
        Name = name;
    }

    public string Name { get;}
}