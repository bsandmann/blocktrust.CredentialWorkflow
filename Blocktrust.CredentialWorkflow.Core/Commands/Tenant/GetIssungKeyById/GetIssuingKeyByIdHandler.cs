using Blocktrust.CredentialWorkflow.Core.Domain.IssuingKey;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Tenant.GetIssungKeyById;

public class GetIssuingKeyByIdHandler : IRequestHandler<GetIssuingKeyByIdRequest, Result<IssuingKey>>
{
    private readonly DataContext _context;

    public GetIssuingKeyByIdHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<IssuingKey>> Handle(GetIssuingKeyByIdRequest request, CancellationToken cancellationToken)
    {
        _context.ChangeTracker.Clear();

        var issuingKeyEntity = await _context.IssuingKeys
            .FirstOrDefaultAsync(i => i.IssuingKeyId == request.IssuingKeyId, cancellationToken);

        if (issuingKeyEntity == null)
        {
            return Result.Fail("Issuing key not found.");
        }

        return Result.Ok(issuingKeyEntity.Map(issuingKeyEntity));
    }
}