﻿namespace Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.ProcessMessage;

using Blocktrust.DIDComm.Message.Messages;

public class ProcessMessageResponse
{
    /// <summary>
    /// The response message
    /// </summary>
    public Message Message { get; }

    /// <summary>
    /// The Did of the current Mediator
    /// </summary>
    // public string? MediatorDid { get; }

    /// <summary>
    /// The http request should be accepted (202)
    /// </summary>
    public bool RespondWithAccepted { get; }

    public ProcessMessageResponse(Message message)
    {
        this.Message = message;
        // this.MediatorDid = mediatorDid;
    }

    public ProcessMessageResponse()
    {
        RespondWithAccepted = true;
    }
}