namespace Blocktrust.CredentialWorkflow.Core.Commands.Tenant.AddIdentusAgent;

using FluentResults;
using MediatR;

public class AddIdentusAgentRequest : IRequest<Result<Guid>>
{
    public AddIdentusAgentRequest(Guid tenantId, string name, Uri uri, string apiKey)
    {
        TenantId = tenantId;
        Name = name;
        Uri = uri;
        ApiKey = apiKey;
    }

    public Guid TenantId { get; }
    public string Name { get; }
    public Uri Uri { get; }
    public string ApiKey { get; }
}