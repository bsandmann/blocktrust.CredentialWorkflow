namespace Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.SavePeerDIDSecrets;

using Blocktrust.Common.Models.Secrets;
using FluentResults;
using MediatR;

public class SavePeerDIDSecretRequest : IRequest<Result>
{
    public Secret Secret { get; }
    public string Kid { get; }

    public SavePeerDIDSecretRequest(string kid, Secret secret)
    {
        Kid = kid;
        Secret = secret;
    }
}