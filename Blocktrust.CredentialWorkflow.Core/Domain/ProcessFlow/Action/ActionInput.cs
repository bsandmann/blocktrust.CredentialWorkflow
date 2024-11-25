using System.Text.Json.Serialization;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Action.Input;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Action;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(ActionInputCredentialIssuance), typeDiscriminator: "credentialIssuance")]
[JsonDerivedType(typeof(ActionInputCredentialVerification), typeDiscriminator: "credentialVerification")]
[JsonDerivedType(typeof(ActionInputOutgoingRequest), typeDiscriminator: "outgoingRequest")]
[JsonDerivedType(typeof(ActionInputDIDCommTrustPing), typeDiscriminator: "didcommTrustPing")]
[JsonDerivedType(typeof(ActionInputDIDCommMessage), typeDiscriminator: "didcommMessage")]
public abstract class ActionInput
{
    [JsonPropertyName("id")] 
    public Guid Id { get; set; }
}