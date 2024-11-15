namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow;

public class WorkflowContext
{
    private readonly Dictionary<string, object> _variables = new();
    private readonly Dictionary<string, ActionResult> _actionResults = new();
    private ActionResult? _triggerResult;

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

    public void SetActionResult(string actionId, ActionResult result)
    {
        _actionResults[actionId] = result;
    }

    public ActionResult? GetActionResult(string actionId)
    {
        return _actionResults.TryGetValue(actionId, out var result) ? result : null;
    }

    public void SetTriggerResult(ActionResult result)
    {
        _triggerResult = result;
    }

    public ActionResult? GetTriggerResult()
    {
        return _triggerResult;
    }
}