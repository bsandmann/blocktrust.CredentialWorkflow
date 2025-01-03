﻿using Blocktrust.CredentialWorkflow.Core.Domain.Common;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Outgoing;

public class OutgoingDIDComm : ActionInput
{
    public ParameterReference TargetDid { get; set; } = new();
    public Dictionary<string, MessageFieldValue> MessageContent { get; set; } = new();
}