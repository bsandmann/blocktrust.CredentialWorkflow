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

    public string? ResolveValue(WorkflowContext context, IConfiguration configuration)
    {
        return Source switch
        {
            ParameterSource.Static => DefaultValue,
            ParameterSource.TriggerInput => ResolveTriggerInput(context),
            ParameterSource.AppSettings => configuration[Path],
            _ => null
        };
    }

    private string? ResolveTriggerInput(WorkflowContext context)
    {
        var triggerResult = context.GetTriggerResult();
        if (triggerResult?.OutputJson == null) return null;

        try
        {
            var triggerOutput = JsonSerializer.Deserialize<JsonElement>(triggerResult.OutputJson);
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
}

public enum ParameterSource
{
    Static,
    TriggerInput,
    AppSettings,
    
    // To do: Actions outcome to specify the action
    ActionOutcome
}