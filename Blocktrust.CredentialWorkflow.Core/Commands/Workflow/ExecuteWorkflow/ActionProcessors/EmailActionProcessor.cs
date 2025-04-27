using System.Text.Json;
using System.Text.RegularExpressions;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.SendEmailAction;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Outgoing;
using FluentResults;
using MediatR;
using Action = Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Action;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow.ActionProcessors;

using Action = Action;

public class EmailActionProcessor : IActionProcessor
{
    private readonly IMediator _mediator;

    public EmailActionProcessor(IMediator mediator)
    {
        _mediator = mediator;
    }

    public EActionType ActionType => EActionType.Email;

    public async Task<Result> ProcessAsync(Action action, ActionOutcome actionOutcome, ActionProcessingContext context)
    {
        var input = (EmailAction)action.Input;

        var toEmail = await ParameterResolver.GetParameterFromExecutionContext(
            input.To, context.ExecutionContext, context.Workflow, context.ActionOutcomes, ActionType, _mediator);
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            var errorMessage = "The recipient email address is not provided in the execution context parameters.";
            actionOutcome.FinishOutcomeWithFailure(errorMessage);
            return Result.Fail(errorMessage);
        }

        var parameters = new Dictionary<string, string>();
        foreach (var param in input.Parameters)
        {
            if (string.IsNullOrEmpty(param.Value.Path))
            {
                param.Value.Path = param.Key;
            }

            var value = await ParameterResolver.GetParameterFromExecutionContext(
                param.Value, context.ExecutionContext, context.Workflow, context.ActionOutcomes, ActionType, _mediator);
            if (value != null)
            {
                parameters[param.Key] = value;
            }
        }

        try
        {
            var subject = ProcessEmailTemplate(input.Subject, parameters);
            var body = ProcessEmailTemplate(input.Body, parameters);

            var sendEmailRequest = new SendEmailActionRequest(toEmail, subject, body);
            var sendResult = await _mediator.Send(sendEmailRequest, context.CancellationToken);

            if (sendResult.IsFailed)
            {
                var errorMessage = sendResult.Errors.FirstOrDefault()?.Message ?? "Failed to send email.";
                actionOutcome.FinishOutcomeWithFailure(errorMessage);
                return Result.Fail(errorMessage);
            }

            actionOutcome.FinishOutcomeWithSuccess("Email sent successfully");
            return Result.Ok();
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error sending email: {ex.Message}";
            actionOutcome.FinishOutcomeWithFailure(errorMessage);
            return Result.Fail(errorMessage);
        }
    }

    public static string ProcessEmailTemplate(string template, Dictionary<string, string> parameters)
    {
        if (string.IsNullOrEmpty(template))
            return string.Empty;

        if (parameters == null || !parameters.Any())
            return template;

        var processedTemplate = template;
        foreach (var param in parameters)
        {
            if (!string.IsNullOrEmpty(param.Key))
            {
                var paramValue = param.Value ?? string.Empty;
                
                // Check if the parameter value is JSON and beautify it if necessary
                if (IsJson(paramValue))
                {
                    paramValue = BeautifyJson(paramValue);
                }
                
                var key = param.Key.Trim();
                // Replace {{paramName}} pattern with the value
                processedTemplate = Regex.Replace(
                    processedTemplate,
                    "\\{\\{" + key + "\\}\\}",
                    paramValue,
                    RegexOptions.IgnoreCase);
            }
        }

        return processedTemplate;
    }
    
    private static bool IsJson(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;
            
        input = input.Trim();
        
        // Quick check for JSON structure
        if (!(input.StartsWith("{") && input.EndsWith("}")) && 
            !(input.StartsWith("[") && input.EndsWith("]")))
            return false;
            
        try
        {
            // Attempt to parse as JSON
            using (JsonDocument.Parse(input))
            {
                return true;
            }
        }
        catch
        {
            return false;
        }
    }
    
    private static string BeautifyJson(string jsonString)
    {
        try
        {
            // Parse and format the JSON with indentation
            using var jsonDocument = JsonDocument.Parse(jsonString);
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            
            return JsonSerializer.Serialize(jsonDocument.RootElement, options);
        }
        catch
        {
            // If there's any error in formatting, return the original string
            return jsonString;
        }
    }
}