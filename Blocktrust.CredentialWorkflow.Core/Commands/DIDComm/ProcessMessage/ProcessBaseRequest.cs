﻿namespace Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.ProcessMessage;

using Blocktrust.DIDComm.Message.FromPriors;
using Blocktrust.DIDComm.Message.Messages;
using MediatR;

/// <summary>
/// Request to Process DIDComm messages for specific message types
/// </summary>
public abstract class ProcessBaseRequest : IRequest<Message?>
{
    public Message UnpackedMessage { get; }
    public string? SenderDid { get; }
    public string? HostUrl { get; }
    public FromPrior? FromPrior { get; }

    public ProcessBaseRequest(Message unpackedMessage, string? senderDid, string hostUrl, FromPrior? fromPrior)
    {
        UnpackedMessage = unpackedMessage;
        SenderDid = senderDid;
        HostUrl = hostUrl;
        FromPrior = fromPrior;
    } 
    
    public ProcessBaseRequest(Message unpackedMessage)
    {
        UnpackedMessage = unpackedMessage;
    }
}