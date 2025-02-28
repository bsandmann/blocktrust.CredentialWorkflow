namespace Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.BasicMessageAB;

using Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.ProcessMessage;
using Blocktrust.DIDComm.Message.FromPriors;
using Blocktrust.DIDComm.Message.Messages;

/// <inheritdoc />
public class BasicMessageAbRequest : ProcessBaseRequest
{
    /// <inheritdoc />
    public BasicMessageAbRequest(Message unpackedMessage, string? senderDid, string hostUrl, FromPrior? fromPrior) : base(unpackedMessage, senderDid, hostUrl, fromPrior)
    {
    }
}