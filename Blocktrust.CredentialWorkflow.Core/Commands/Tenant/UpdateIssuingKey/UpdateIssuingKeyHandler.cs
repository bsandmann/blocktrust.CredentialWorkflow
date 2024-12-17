using Blocktrust.CredentialWorkflow.Core;
using Blocktrust.CredentialWorkflow.Core.Domain.IssuingKey;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class UpdateIssuingKeyHandler : IRequestHandler<UpdateIssuingKeyRequest, Result<IssuingKey>>
{
    private readonly DataContext _context;

    public UpdateIssuingKeyHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<IssuingKey>> Handle(UpdateIssuingKeyRequest request, CancellationToken cancellationToken)
    {
        _context.ChangeTracker.Clear();

        var issuingKey = await _context.IssuingKeys
            .FirstOrDefaultAsync(i => i.IssuingKeyId == request.IssuingKeyId, cancellationToken);

        if (issuingKey is null)
        {
            return Result.Fail("The IssuingKey does not exist in the database. It cannot be updated.");
        }

        // Update fields if provided
        if (!string.IsNullOrWhiteSpace(request.Name))
            issuingKey.Name = request.Name;

        if (!string.IsNullOrWhiteSpace(request.KeyType))
            issuingKey.KeyType = request.KeyType;

        if (!string.IsNullOrWhiteSpace(request.PublicKey))
            issuingKey.PublicKey = request.PublicKey;

        if (!string.IsNullOrWhiteSpace(request.PrivateKey))
            issuingKey.PrivateKey = request.PrivateKey;

        _context.IssuingKeys.Update(issuingKey);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok(issuingKey.Map(issuingKey));
    }
}