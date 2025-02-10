namespace Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.GetPeerDIDSecrets;

using Blocktrust.Common.Models.DidDoc;
using Blocktrust.Common.Models.Secrets;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetPeerDIDSecretsHandler : IRequestHandler<GetPeerDIDSecretsRequest, Result<List<Secret>>>
{
    private readonly DataContext _context;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context"></param>
    public GetPeerDIDSecretsHandler(DataContext context)
    {
        this._context = context;
    }

    /// <summary>
    /// Handler
    /// </summary>
    /// <param name="savePeerDidSecretsRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result<List<Secret>>> Handle(GetPeerDIDSecretsRequest savePeerDidSecretsRequest, CancellationToken cancellationToken)
    {
        var secretEntities = await _context.PeerDIDSecrets.Where(p => savePeerDidSecretsRequest.Kids.Contains(p.Kid)).ToListAsync(cancellationToken: cancellationToken);

        return Result.Ok(secretEntities.Select(p => new Secret(
            kid: p.Kid,
            type: (VerificationMethodType)p.VerificationMethodType,
            verificationMaterial: new VerificationMaterial(
                format: (VerificationMaterialFormat)p.VerificationMaterialFormat,
                value: p.Value
            )
        )).ToList());
    }
}