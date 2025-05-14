using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Outgoing;
using FluentResults;
using MediatR;

namespace Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow.ActionProcessors;

public class HttpActionProcessor : IActionProcessor
{
    private readonly IMediator _mediator;
    private readonly HttpClient _httpClient;

    public HttpActionProcessor(IMediator mediator, IHttpClientFactory httpClientFactory)
    {
        _mediator = mediator;
        _httpClient = httpClientFactory.CreateClient("HttpActionProcessor");
    }

    public EActionType ActionType => EActionType.Http;

    public async Task<Result> ProcessAsync(Domain.ProcessFlow.Actions.Action action, ActionOutcome actionOutcome, ActionProcessingContext context)
    {
        var input = (HttpAction)action.Input;

        try
        {
            // Resolve the endpoint URL
            var endpoint = await ParameterResolver.GetParameterFromExecutionContext(
                input.Endpoint, context.ExecutionContext, context.Workflow, context.ActionOutcomes, ActionType, _mediator);
            
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                var errorMessage = "The endpoint URL is not provided.";
                actionOutcome.FinishOutcomeWithFailure(errorMessage);
                return Result.Fail(errorMessage);
            }

            // Create HTTP request message
            var request = new HttpRequestMessage
            {
                Method = new HttpMethod(input.Method)
            };

            // Set request URI
            if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri))
            {
                var errorMessage = $"Invalid endpoint URL: {endpoint}";
                actionOutcome.FinishOutcomeWithFailure(errorMessage);
                return Result.Fail(errorMessage);
            }
            request.RequestUri = uri;

            // Track content type for later use
            var contentType = "application/json";

            // Process headers
            foreach (var header in input.Headers)
            {
                var headerValue = await ParameterResolver.GetParameterFromExecutionContext(
                    header.Value, context.ExecutionContext, context.Workflow, context.ActionOutcomes, ActionType, _mediator);
                
                if (!string.IsNullOrEmpty(headerValue))
                {
                    // Handle special headers that need to be set on specific properties
                    if (header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                    {
                        // Store the content type to use when creating content
                        contentType = headerValue;
                    }
                    else if (header.Key.Equals("Accept", StringComparison.OrdinalIgnoreCase))
                    {
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(headerValue));
                    }
                    else if (header.Key.Equals("User-Agent", StringComparison.OrdinalIgnoreCase))
                    {
                        request.Headers.UserAgent.ParseAdd(headerValue);
                    }
                    else
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, headerValue);
                    }
                }
            }

            // Process body if method supports it
            if (input.Method != "GET" && input.Method != "DELETE")
            {
                // Get parameters for substitution
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

                // Check if we have a body content
                string bodyContent = string.Empty;
                if (input.Body.TryGetValue("__body", out var bodyRef))
                {
                    bodyContent = bodyRef.Path;
                }

                // Process template with parameters
                bodyContent = ProcessTemplate(bodyContent, parameters);

                // Create and set content with the specified content type
                request.Content = new StringContent(bodyContent, Encoding.UTF8, contentType);
            }

            // Send request
            var response = await _httpClient.SendAsync(request, context.CancellationToken);

            // Read response
            var responseContent = await response.Content.ReadAsStringAsync(context.CancellationToken);

            // Create result
            var result = new
            {
                StatusCode = (int)response.StatusCode,
                IsSuccessStatusCode = response.IsSuccessStatusCode,
                ReasonPhrase = response.ReasonPhrase,
                Content = responseContent,
                Headers = response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value))
            };

            // Convert to JSON for storage
            var resultJson = JsonSerializer.Serialize(result);

            if (response.IsSuccessStatusCode)
            {
                // Provide a success message with additional data
                var successMessage = $"HTTP request completed with status code {(int)response.StatusCode}";
                
                // Include first 100 chars of response in the message (if not too large)
                if (!string.IsNullOrEmpty(responseContent) && responseContent.Length <= 100)
                {
                    successMessage += $": {responseContent}";
                }
                else if (!string.IsNullOrEmpty(responseContent))
                {
                    successMessage += $": {responseContent.Substring(0, 100)}...";
                }
                
                actionOutcome.FinishOutcomeWithSuccess(resultJson);
                return Result.Ok();
            }
            else
            {
                var errorMessage = $"HTTP request failed with status code {(int)response.StatusCode}: {response.ReasonPhrase}";
                actionOutcome.FinishOutcomeWithFailure(errorMessage);
                return Result.Fail(errorMessage);
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error executing HTTP request: {ex.Message}";
            actionOutcome.FinishOutcomeWithFailure(errorMessage);
            return Result.Fail(errorMessage);
        }
    }

    private static string ProcessTemplate(string template, Dictionary<string, string> parameters)
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
                
                // Check if the parameter value is JSON and format it consistently
                if (IsJson(paramValue))
                {
                    paramValue = FormatJson(paramValue);
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
    
    private static string FormatJson(string jsonString)
    {
        try
        {
            // We keep the formatting minimal for HTTP requests
            var options = new JsonSerializerOptions
            {
                WriteIndented = false
            };
            
            using var jsonDocument = JsonDocument.Parse(jsonString);
            return JsonSerializer.Serialize(jsonDocument.RootElement, options);
        }
        catch
        {
            // If there's any error in formatting, return the original string
            return jsonString;
        }
    }
}