using Blocktrust.Common.Models.Secrets;
using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.GetPeerDIDSecrets;

public class GetPeerDIDSecretsRequest : IRequest<Result<List<Secret>>>
{
    public List<string> Kids { get; }

    public GetPeerDIDSecretsRequest(List<string> kids)
    {
        Kids = kids;
    }
}