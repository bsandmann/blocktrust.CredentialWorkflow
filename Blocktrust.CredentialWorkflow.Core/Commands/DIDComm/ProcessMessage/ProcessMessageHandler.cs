namespace Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.ProcessMessage;

using BasicMessageWorkflow;
using Blocktrust.DIDComm.Message.FromPriors;
using Blocktrust.DIDComm.Message.Messages;
using Blocktrust.Mediator.Common.Models.ProblemReport;
using Blocktrust.Mediator.Common.Protocols;
using MediatR;
using TrustPing;

public class ProcessMessageHandler : IRequestHandler<ProcessMessageRequest, ProcessMessageResponse>
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Constructor
    /// </summary>
    public ProcessMessageHandler(IMediator mediator)
    {
        this._mediator = mediator;
    }

    /// <inheritdoc />
    public async Task<ProcessMessageResponse> Handle(ProcessMessageRequest request, CancellationToken cancellationToken)
    {
        FromPrior? fromPrior = null;
        var fallBackMediatorDidForErrorMessages = request.UnpackResult.Metadata?.EncryptedTo?.FirstOrDefault();
        if (fallBackMediatorDidForErrorMessages is null)
        {
            return new ProcessMessageResponse(null);
        }

        Message? result;
        switch (request.UnpackResult.Message.Type)
        {
            case ProtocolConstants.TrustPingRequest:
                result = await _mediator.Send(new ProcessTrustPingRequest(request.UnpackResult.Message), cancellationToken);
                break;
            case ProtocolConstants.BasicMessage:
                result = await _mediator.Send(new BasicMessageWorkflowRequest(request.UnpackResult.Message, request.SenderDid,  request.HostUrl, fromPrior, request.Workflow), cancellationToken);
                break;
            default:
                return new ProcessMessageResponse(ProblemReportMessage.Build(
                    fromPrior: fromPrior,
                    threadIdWhichCausedTheProblem: request.UnpackResult.Message.Thid ?? request.UnpackResult.Message.Id,
                    problemCode: new ProblemCode(
                        sorter: EnumProblemCodeSorter.Error,
                        scope: EnumProblemCodeScope.Message,
                        stateNameForScope: null,
                        descriptor: EnumProblemCodeDescriptor.Message,
                        otherDescriptor: null
                    ),
                    comment: $"Not supported message type: '{request.UnpackResult.Message.Type}'",
                    commentArguments: null,
                    escalateTo: new Uri("mailto:info@blocktrust.dev")));
        }

        if (result is null)
        {
            return new ProcessMessageResponse(); // 202 Accecpted
        }

        return new ProcessMessageResponse(result);
    }
}