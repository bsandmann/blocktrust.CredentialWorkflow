using Blocktrust.CredentialWorkflow.Core.Domain.Handlers;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Action;

namespace Blocktrust.CredentialWorkflow.Core.Factories;

public interface IActionHandlerFactory
{
    IActionHandler? GetHandler(EActionType actionType);
}