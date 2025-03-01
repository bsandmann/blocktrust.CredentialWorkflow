namespace Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.BasicMessageWorkflow;

using Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.ProcessMessage;
using Blocktrust.DIDComm.Message.FromPriors;
using Blocktrust.DIDComm.Message.Messages;
using Domain.Workflow;

/// <inheritdoc />
public class BasicMessageWorkflowRequest : ProcessBaseRequest
{
    /// <inheritdoc />
    public BasicMessageWorkflowRequest(Message unpackedMessage, string? senderDid, string hostUrl, FromPrior? fromPrior, Workflow requestWorkflow) : base(unpackedMessage, senderDid, hostUrl, fromPrior)
    {
        Workflow = requestWorkflow;
    }

    public Workflow Workflow { get; }
}