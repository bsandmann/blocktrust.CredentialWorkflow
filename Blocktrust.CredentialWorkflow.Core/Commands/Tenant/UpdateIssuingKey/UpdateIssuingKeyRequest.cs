using Blocktrust.CredentialWorkflow.Core.Domain.IssuingKey;
using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Tenant.UpdateIssuingKey;

public class UpdateIssuingKeyRequest : IRequest<Result<IssuingKey>>
{
    public UpdateIssuingKeyRequest(Guid issuingKeyId, string? name = null, string? did = null, string? keyType = null, string? publicKey = null, string? privateKey = null)
    {
        IssuingKeyId = issuingKeyId;
        Name = name;
        Did = did;
        KeyType = keyType;
        PublicKey = publicKey;
        PrivateKey = privateKey;
    }

    public Guid IssuingKeyId { get; }
    public string? Name { get; }
    public string? Did { get; }
    public string? KeyType { get; }
    public string? PublicKey { get; }
    public string? PrivateKey { get; }
}