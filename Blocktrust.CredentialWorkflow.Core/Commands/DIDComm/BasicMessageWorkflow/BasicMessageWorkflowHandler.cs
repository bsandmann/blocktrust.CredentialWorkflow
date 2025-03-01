namespace Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.BasicMessageWorkflow;

using System.Text.Json;
using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.Mediator.Common.Models.ProblemReport;
using Blocktrust.Mediator.Common.Protocols;
using MediatR;
using Blocktrust.CredentialWorkflow.Core.Domain.Common;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Triggers;
using Blocktrust.CredentialWorkflow.Core.Commands.WorkflowOutcome.CreateWorkflowOutcome;
using Blocktrust.CredentialWorkflow.Core.Services;

public class BasicMessageWorkflowHandler : IRequestHandler<BasicMessageWorkflowRequest, Message?>
{
    private readonly IMediator _mediator;
    private readonly IWorkflowQueue _workflowQueue;

    public BasicMessageWorkflowHandler(IMediator mediator, IWorkflowQueue workflowQueue)
    {
        _mediator = mediator;
        _workflowQueue = workflowQueue;
    }

    /// <inheritdoc />
    public async Task<Message?> Handle(BasicMessageWorkflowRequest request, CancellationToken cancellationToken)
    {
        var body = request.UnpackedMessage.Body;
        string content = String.Empty;
        var hasContent = body.TryGetValue("content", out var contentBody);
        if (hasContent)
        {
            var contentJsonElement = (JsonElement)contentBody!;
            if (contentJsonElement.ValueKind is JsonValueKind.String)
            {
                content = contentJsonElement.GetString() ?? string.Empty;
            }
            else
            {
                return ProblemReportMessage.BuildDefaultMessageMissingArguments(
                    errorMessage: "Invalid body format: missing content",
                    threadIdWhichCausedTheProblem: request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id,
                    fromPrior: request.FromPrior);
            }
        }

        var workflowId = request.Workflow.WorkflowId;
        var errors = new List<string>();

        // Parse the content as JSON
        JsonElement contentJson;
        try
        {
            contentJson = JsonSerializer.Deserialize<JsonElement>(content);
        }
        catch (JsonException)
        {
            errors.Add("Content is not valid JSON");
            return BuildResponseMessage(request, workflowId, errors);
        }

        // Get the trigger - it should be a WalletInteractionTrigger
        var processFlow = request.Workflow.ProcessFlow;
        if (processFlow is null || !processFlow.Triggers.Any())
        {
            errors.Add("The workflow does not have a trigger");
            return BuildResponseMessage(request, workflowId, errors);
        }

        var trigger = processFlow.Triggers.First().Value;
        if (trigger.Type != ETriggerType.WalletInteraction)
        {
            errors.Add("The workflow trigger is not a wallet interaction trigger");
            return BuildResponseMessage(request, workflowId, errors);
        }

        // Verify required parameters are present
        var walletInteractionTrigger = (TriggerInputWalletInteraction)trigger.Input;
        foreach (var requiredParam in walletInteractionTrigger.RequiredParameters)
        {
            var paramKey = requiredParam.Key;
            var paramDef = requiredParam.Value;

            if (paramDef.Required && !contentJson.TryGetProperty(paramKey, out var _))
            {
                errors.Add($"Required parameter '{paramKey}' is missing");
            }
        }

        if (errors.Any())
        {
            return BuildResponseMessage(request, workflowId, errors);
        }

        // Create a SimplifiedHttpContext to store the execution context
        var simplifiedContext = new SimplifiedHttpContext
        {
            Method = HttpRequestMethod.POST,
            Body = content
        };

        // Create execution context from the HTTP context
        var executionContext = simplifiedContext.ToJson();

        // Create workflow outcome and enqueue it for processing
        var outcomeResult = await _mediator.Send(new CreateWorkflowOutcomeRequest(workflowId, executionContext), cancellationToken);
        if (outcomeResult.IsFailed)
        {
            errors.Add($"Failed to create outcome for workflow {workflowId}: {outcomeResult.Errors.FirstOrDefault()?.Message}");
            return BuildResponseMessage(request, workflowId, errors);
        }

        // Enqueue the workflow for execution
        await _workflowQueue.EnqueueAsync(outcomeResult.Value);

        // Return success message
        return BuildResponseMessage(request, workflowId, errors);
    }

    private Message BuildResponseMessage(BasicMessageWorkflowRequest request, Guid workflowId, List<string> errors)
    {
        var returnBody = new Dictionary<string, object>();
        if (errors.Any())
        {
            returnBody.Add("content", $"Workflow '{workflowId}' was triggered, but errors occurred: {string.Join(", ", errors)}");
        }
        else
        {
            returnBody.Add("content", $"Workflow '{workflowId}' was triggered successfully");
        }

        return new MessageBuilder(
                id: Guid.NewGuid().ToString(),
                type: ProtocolConstants.BasicMessage,
                body: returnBody
            )
            .thid(request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id)
            .fromPrior(request.FromPrior)
            .build();
    }
}