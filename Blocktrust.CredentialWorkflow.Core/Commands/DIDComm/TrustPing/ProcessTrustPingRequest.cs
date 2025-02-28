namespace Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.TrustPing;

using Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.ProcessMessage;
using Blocktrust.DIDComm.Message.Messages;

/// <inheritdoc />
public class ProcessTrustPingRequest : ProcessBaseRequest
{
    /// <inheritdoc />
    public ProcessTrustPingRequest(Message unpackedMessage) : base(unpackedMessage)
    {
    }
}