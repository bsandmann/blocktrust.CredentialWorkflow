using Blocktrust.CredentialWorkflow.Core.Domain.IssuingKey;
using Blocktrust.CredentialWorkflow.Core.Entities.Tenant;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Tenant.CreateIssuingKey;

using Prism;

public class CreateIssuingKeyHandler : IRequestHandler<CreateIssuingKeyRequest, Result<IssuingKey>>
{
    private readonly DataContext _context;

    public CreateIssuingKeyHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<IssuingKey>> Handle(CreateIssuingKeyRequest request, CancellationToken cancellationToken)
    {
        _context.ChangeTracker.Clear();
        _context.ChangeTracker.AutoDetectChangesEnabled = false;

        var tenant = await _context.TenantEntities
            .FirstOrDefaultAsync(t => t.TenantEntityId == request.TenantId, cancellationToken);

        if (tenant is null)
        {
            return Result.Fail("The tenant does not exist in the database. The IssuingKey cannot be created.");
        }

        if (!string.IsNullOrWhiteSpace(request.PublicKey2))
        {
            try
            {
                var KeyX = PrismEncoding.Base64ToByteArray(request.PublicKey);
                var KeyY = PrismEncoding.Base64ToByteArray(request.PublicKey2);
                var compressedKey = CompressPublicKey(KeyX, KeyY, "secp256k1");
                if (compressedKey.Length != 33)
                {
                    return Result.Fail("The compressed key is not 33 bytes long.");
                }

                request.PublicKey = PrismEncoding.ByteArrayToBase64(compressedKey);
            }
            catch (Exception ex)
            {
                return Result.Fail($"Error converting public key: {ex.Message}");
            }
        }

        var issuingKey = new IssuingKeyEntity
        {
            IssuingKeyId = Guid.NewGuid(),
            Name = request.Name,
            Did = request.Did,
            CreatedUtc = DateTime.UtcNow,
            KeyType = request.KeyType,
            PublicKey = request.PublicKey,
            PrivateKey = request.PrivateKey
        };

        issuingKey.TenantEntityId = tenant.TenantEntityId;

        await _context.IssuingKeys.AddAsync(issuingKey, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok(issuingKey.Map(issuingKey));
    }

    public static byte[] CompressPublicKey(byte[] x, byte[] y, string curve )
    {
        if(curve != "secp256k1")
        {
            throw new Exception("Only secp256k1 is supported");
        }

        byte[] newArray = new byte[x.Length + 1];
        x.CopyTo(newArray, 1);
        newArray[0] = (byte)(2 + (y[^1] & 1));
        return newArray;
    }
}