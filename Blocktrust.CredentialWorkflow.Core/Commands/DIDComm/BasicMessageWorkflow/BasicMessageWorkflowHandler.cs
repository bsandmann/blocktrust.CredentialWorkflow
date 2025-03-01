namespace Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.BasicMessageWorkflow;

using System.Text.Json;
using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.Mediator.Common.Models.ProblemReport;
using Blocktrust.Mediator.Common.Protocols;
using MediatR;

public class BasicMessageWorkflowHandler : IRequestHandler<BasicMessageWorkflowRequest, Message?>
{
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

        // the content-string should be a json -> verify
        // Then verify that it the reuqireed inputs from the worflow are in there
        // then add it to the queue to be executed, same as the when the request would have been come over the WorkflowController.
        // all erros should be collected and send back below


        var returnBody = new Dictionary<string, object>();
        if (errors.Any())
        {
            // TODO properly list the erros
            returnBody.Add("content", $"Worflow '{workflowId}' was triggered, but errors occured: {errors}");
        }
        else
        {
            returnBody.Add("content", $"Worflow '{workflowId}' was triggered sucesscullyk");
        }

        var returnMessage = new MessageBuilder(
                id: Guid.NewGuid().ToString(),
                type: ProtocolConstants.BasicMessage,
                body: returnBody
            )
            .thid(request.UnpackedMessage.Thid ?? request.UnpackedMessage.Id)
            .fromPrior(request.FromPrior)
            .build();

        return returnMessage;
    }
}