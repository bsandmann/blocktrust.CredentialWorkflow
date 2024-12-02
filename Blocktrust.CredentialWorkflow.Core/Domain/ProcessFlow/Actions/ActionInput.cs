using System.Text.Json.Serialization;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Issuance;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Outgoing;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Verification;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]

[JsonDerivedType(typeof(IssueW3cCredential), typeDiscriminator: "issueW3cCredential")]
[JsonDerivedType(typeof(IssueW3CSdCredential), typeDiscriminator: "w3cSdCredential")]
[JsonDerivedType(typeof(IssueAnoncredCredential), typeDiscriminator: "anoncredCredential")]

[JsonDerivedType(typeof(VerifyW3cCredential), typeDiscriminator: "verifyW3cCredential")]
[JsonDerivedType(typeof(VerifyW3cSdCredential), typeDiscriminator: "verifyW3cSdCredential")]
[JsonDerivedType(typeof(VerifyAnoncredCredential), typeDiscriminator: "verifyAnoncredCredential")]


[JsonDerivedType(typeof(OutgoingDIDComm), typeDiscriminator: "outgoingDIDCommMessage")]

public abstract class ActionInput
{
    [JsonPropertyName("id")] 
    public Guid Id { get; set; }
}