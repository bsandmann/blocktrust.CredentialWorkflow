using Blocktrust.CredentialWorkflow.Core.Domain.Common;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow;

using Actions;

public class WorkflowContext
{
    private readonly Dictionary<string, object> _variables = new();
    private readonly Dictionary<string, ActionOutcome> _actionResults = new();
    private ActionOutcome? _triggerResult;

    public void SetVariable(string name, object value)
    {
        _variables[name] = value;
    }

    public T? GetVariable<T>(string name)
    {
        if (_variables.TryGetValue(name, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return default;
    }

    public void SetActionResult(string actionId, ActionOutcome result)
    {
        _actionResults[actionId] = result;
    }

    public ActionOutcome? GetActionResult(string actionId)
    {
        return _actionResults.TryGetValue(actionId, out var result) ? result : null;
    }

    public void SetTriggerResult(ActionOutcome result)
    {
        _triggerResult = result;
    }

    public ActionOutcome? GetTriggerResult()
    {
        return _triggerResult;
    }
}