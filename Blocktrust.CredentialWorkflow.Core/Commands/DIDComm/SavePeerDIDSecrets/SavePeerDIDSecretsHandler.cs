namespace Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.SavePeerDIDSecrets;

using Blocktrust.CredentialWorkflow.Core.Entities.DIDComm;
using FluentResults;
using MediatR;

public class SavePeerDIDSecretsHandler : IRequestHandler<SavePeerDIDSecretRequest, Result>
{
    private readonly DataContext _context;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context"></param>
    public SavePeerDIDSecretsHandler(DataContext context)
    {
        this._context = context;
    }

    /// <summary>
    /// Handler
    /// </summary>
    /// <param name="savePeerDidSecretRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result> Handle(SavePeerDIDSecretRequest savePeerDidSecretRequest, CancellationToken cancellationToken)
    {
        var secretEntity = new PeerDIDSecretEntity()
        {
            CreatedUtc = DateTime.UtcNow,
            Kid = savePeerDidSecretRequest.Kid,
            Value = savePeerDidSecretRequest.Secret.VerificationMaterial.Value,
            VerificationMaterialFormat = (int)savePeerDidSecretRequest.Secret.VerificationMaterial.Format,
            VerificationMethodType = (int)savePeerDidSecretRequest.Secret.Type
        };
        await _context.AddAsync(secretEntity, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}