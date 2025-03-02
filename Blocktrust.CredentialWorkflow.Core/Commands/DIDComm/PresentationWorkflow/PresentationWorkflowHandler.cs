namespace Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.PresentationWorkflow;

using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks.Sources;
using BasicMessageWorkflow;
using Blocktrust.CredentialWorkflow.Core.Commands.WorkflowOutcome.CreateWorkflowOutcome;
using Blocktrust.CredentialWorkflow.Core.Domain.Common;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Triggers;
using Blocktrust.CredentialWorkflow.Core.Services;
using Blocktrust.DIDComm.Message.Attachments;
using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.Mediator.Common.Models.ProblemReport;
using Blocktrust.Mediator.Common.Protocols;
using Common.Converter;
using FluentResults;
using Mediator.Common.Models.Credential;
using Mediator.Common.Models.CredentialOffer;
using MediatR;
using PeerDID.Types;

public class PresentationWorkflowHandler : IRequestHandler<BasicMessageWorkflowRequest, Message?>
{
    private readonly IMediator _mediator;
    private readonly IWorkflowQueue _workflowQueue;

    public PresentationWorkflowHandler(IMediator mediator, IWorkflowQueue workflowQueue)
    {
        _mediator = mediator;
        _workflowQueue = workflowQueue;
    }

    /// <inheritdoc />
    public async Task<Message?> Handle(BasicMessageWorkflowRequest request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        var workflowId = request.Workflow.WorkflowId;
        if (request.UnpackedMessage.Attachments is null || request.UnpackedMessage.Attachments.Count != 1)
        {
            errors.Add("The code is currently based on the assumption that there is only one attachment.");
            return BuildResponseMessage(request, workflowId, errors);
        }

        var content = string.Empty;
        foreach (var attachment in request.UnpackedMessage.Attachments)
        {
            Debug.Assert(attachment is not null);
            var id = attachment.Id ?? Guid.NewGuid().ToString();
            var data = attachment.Data;
            if (data is Base64 base64AttachmentData)
            {
                var base64PresentationString = base64AttachmentData?.Base64String;
                Debug.Assert(base64PresentationString is not null);
                var base64PresentationDecode = Base64Url.Decode(base64PresentationString);
                var jwtPresentation = Encoding.UTF8.GetString(base64PresentationDecode);

                var jwtPresentationHeader = jwtPresentation.Split('.')[0];
                var jwtPresentationPayload = jwtPresentation.Split('.')[1];
                var jwtPresentationSignature = jwtPresentation.Split('.')[2];

                var jwtPresentationPayloadedDecoded = Base64Url.Decode(jwtPresentationPayload);
                var jwtPresentationPayloadString = Encoding.UTF8.GetString(jwtPresentationPayloadedDecoded);

                var presentationAttachment = JsonSerializer.Deserialize<AcceptPresentationRequestAttachmentDataStructure>(jwtPresentationPayloadString);
                Debug.Assert(presentationAttachment is not null);
                var nonce = Guid.Parse(presentationAttachment.Nonce);

                var verifiableCredentials = presentationAttachment.Vp.VerifiableCredentials;

                if (verifiableCredentials.Count != 1)
                {
                    errors.Add("Unsupported number of presentations");
                    return BuildResponseMessage(request, workflowId, errors);
                }

                content = verifiableCredentials.Single();
            }
            else
            {
                errors.Add("Content is not valid attachment format");
                return BuildResponseMessage(request, workflowId, errors);
            }
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