using Blocktrust.CredentialWorkflow.Core.Domain.Models.Did;
using FluentResults;

namespace Blocktrust.CredentialWorkflow.Core.Services.Interfaces;

public interface IDidResolutionService
{
    Task<Result<DidDocument>> ResolveDid(string did);
}