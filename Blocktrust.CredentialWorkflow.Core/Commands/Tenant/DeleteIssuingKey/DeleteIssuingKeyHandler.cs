using Blocktrust.CredentialWorkflow.Core;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class DeleteIssuingKeyHandler : IRequestHandler<DeleteIssuingKeyRequest, Result>
{
    private readonly DataContext _context;

    public DeleteIssuingKeyHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteIssuingKeyRequest request, CancellationToken cancellationToken)
    {
        _context.ChangeTracker.Clear();

        var issuingKey = await _context.IssuingKeys
            .FirstOrDefaultAsync(i => i.IssuingKeyId == request.IssuingKeyId, cancellationToken);

        if (issuingKey is null)
        {
            return Result.Fail("The IssuingKey does not exist in the database. It cannot be deleted.");
        }

        _context.IssuingKeys.Remove(issuingKey);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}