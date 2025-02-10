namespace Blocktrust.CredentialWorkflow.Core.Services.DIDComm;

using Blocktrust.Common.Models.Secrets;
using Blocktrust.Common.Resolver;
using Commands.DIDComm.GetPeerDIDSecrets;
using Commands.DIDComm.SavePeerDIDSecrets;
using MediatR;

public class PeerDIDSecretResolver : ISecretResolver
{
    private readonly IMediator _mediator;

    public PeerDIDSecretResolver(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Secret?> FindKey(string kid)
    {
        var secretResults = await _mediator.Send(new GetPeerDIDSecretsRequest(new List<string>() { kid }));
        if (secretResults.IsFailed)
        {
            return null;
        }

        return secretResults.Value.FirstOrDefault();
    }

    public async Task<HashSet<string>> FindKeys(List<string> kids)
    {
        var secretResults =await _mediator.Send(new GetPeerDIDSecretsRequest(kids));
        return secretResults.Value.Select(p => p.Kid).ToHashSet();
    }

    public Task AddKey(string kid, Secret secret)
    {
        return _mediator.Send(new SavePeerDIDSecretRequest(kid: kid, secret: secret));
    }
}