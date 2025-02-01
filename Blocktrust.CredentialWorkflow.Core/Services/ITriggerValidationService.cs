namespace Blocktrust.CredentialWorkflow.Core.Services;

using Domain.Common;
using Domain.ProcessFlow.Triggers;
using FluentResults;

public interface ITriggerValidationService
{
    Result Validate(TriggerInputHttpRequest triggerInput, SimplifiedHttpContext httpContext);
}