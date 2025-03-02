namespace Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.PresentationWorkflow;

using Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.ProcessMessage;
using Blocktrust.CredentialWorkflow.Core.Domain.Workflow;
using Blocktrust.DIDComm.Message.FromPriors;
using Blocktrust.DIDComm.Message.Messages;

/// <inheritdoc />
public class PresentationWorkflowRequest : ProcessBaseRequest
{
    /// <inheritdoc />
    public PresentationWorkflowRequest(Message unpackedMessage, string? senderDid, string hostUrl, FromPrior? fromPrior, Workflow requestWorkflow) : base(unpackedMessage, senderDid, hostUrl, fromPrior)
    {
        Workflow = requestWorkflow;
    }

    public Workflow Workflow { get; }
}