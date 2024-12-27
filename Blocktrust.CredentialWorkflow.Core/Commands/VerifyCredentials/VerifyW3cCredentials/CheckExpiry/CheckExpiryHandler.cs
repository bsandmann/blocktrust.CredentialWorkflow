using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.VerifyCredentials.VerifyW3cCredentials.CheckExpiry;

public class CheckExpiryHandler : IRequestHandler<CheckExpiryRequest, Result<bool>>
{
    public Task<Result<bool>> Handle(CheckExpiryRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Check ValidUntil first
            if (request.Credential.ValidUntil.HasValue)
            {
                return Task.FromResult(Result.Ok(DateTime.UtcNow > request.Credential.ValidUntil.Value));
            }

            // If no ValidUntil, check ExpirationDate
            if (request.Credential.ExpirationDate.HasValue)
            {
                return Task.FromResult(Result.Ok(DateTime.UtcNow > request.Credential.ExpirationDate.Value));
            }

            // If neither exists, credential is not expired
            return Task.FromResult(Result.Ok(false));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result.Fail<bool>(new Error("Failed to check expiry status").CausedBy(ex)));
        }
    }
}