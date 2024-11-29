using Blocktrust.CredentialWorkflow.Core.Domain.Did;
using FluentResults;

namespace Blocktrust.CredentialWorkflow.Core.Services.Interfaces;

public interface IDidResolutionService
{
    Task<Result<DidDocument>> ResolveDid(string did);
}