using Blocktrust.CredentialWorkflow.Core.Domain.Handlers;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Action;
using Microsoft.Extensions.DependencyInjection;

namespace Blocktrust.CredentialWorkflow.Core.Factories;

public class ActionHandlerFactory : IActionHandlerFactory
{
    private readonly Dictionary<EActionType, Type> _handlerTypes;
    private readonly IServiceProvider _serviceProvider;

    public ActionHandlerFactory(
        Dictionary<EActionType, Type> handlerTypes,
        IServiceProvider serviceProvider)
    {
        _handlerTypes = handlerTypes;
        _serviceProvider = serviceProvider;
    }

    public IActionHandler? GetHandler(EActionType actionType)
    {
        if (_handlerTypes.TryGetValue(actionType, out var handlerType))
        {
            return (IActionHandler)ActivatorUtilities.CreateInstance(_serviceProvider, handlerType);
        }
        return null;
    }
}