using System.Text.Json;
using System.Text.Json.Serialization;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow;
using Microsoft.Extensions.Configuration;

namespace Blocktrust.CredentialWorkflow.Core.Domain.Common;

public class ParameterReference
{
    [JsonPropertyName("source")]
    public ParameterSource Source { get; set; }

    [JsonPropertyName("path")]
    public string Path { get; set; } = "";

    [JsonPropertyName("defaultValue")]
    public string? DefaultValue { get; set; }

    [JsonPropertyName("actionId")]
    public Guid? ActionId { get; set; }

    public string? ResolveValue(WorkflowContext context, IConfiguration configuration)
    {
        return Source switch
        {
            ParameterSource.Static => DefaultValue,
            ParameterSource.TriggerInput => ResolveTriggerInput(context),
            ParameterSource.AppSettings => configuration[Path],
            ParameterSource.ActionOutcome => ResolveActionOutcome(context),
            _ => null
        };
    }

    private string? ResolveTriggerInput(WorkflowContext context)
    {
        var triggerResult = context.GetTriggerResult();
        if (triggerResult?.OutcomeJson == null) return null;

        try
        {
            var triggerOutput = JsonSerializer.Deserialize<JsonElement>(triggerResult.OutcomeJson);
            if (triggerOutput.TryGetProperty(Path, out var value))
            {
                return value.GetString();
            }
        }
        catch
        {
            // Log error or handle parsing failure
        }
        return null;
    }

    private string? ResolveActionOutcome(WorkflowContext context)
    {
        // Suppose your WorkflowContext has a way to retrieve the output
        // from a previous action’s result. For example:
        // context.GetActionResult(Guid actionId), which might return JSON
        // or some typed object describing the outcome.
        //
        // The 'Path' might be the property name inside that JSON or
        // some other scheme you define for referencing that data.
        // Alternatively, you might want your `ParameterReference`
        // to hold both an ActionId and a property path. For example:
        //   public Guid? ActionId { get; set; }
        // so that you can do something like:
        //   var actionResult = context.GetActionResult(ActionId.Value);

        // For illustration, we’ll assume `Path` contains the action’s ID
        // and we store property references separately or in some structured way.
        // This is purely an example, you can adapt it as needed.

        if (!Guid.TryParse(Path, out var actionId))
            return null;

        // Retrieve the result of the action
        var actionResult = context.GetActionResult(actionId.ToString());

        if (actionResult?.OutcomeJson == null)
            return null;

        try
        {
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(actionResult.OutcomeJson);

            // If the entire actionResult.OutcomeJson is the credential,
            // you might just return the raw string. If you have subfields,
            // you'd parse them accordingly:
            // e.g. if the credential is in a property named "credential":
            if (jsonElement.TryGetProperty("credential", out var value))
            {
                return value.GetString();
            }
        }
        catch
        {
            // handle or log the error
        }

        return null;
    }
}

public enum ParameterSource
{
    Static,
    TriggerInput,
    AppSettings,
    
    // To do: Actions outcome to specify the action
    ActionOutcome
}