using System.Text.Json.Serialization;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Action.Input;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Action;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(ActionInputCredentialIssuance), typeDiscriminator: "credentialIssuance")]
[JsonDerivedType(typeof(ActionInputCredentialVerification), typeDiscriminator: "credentialVerification")]
[JsonDerivedType(typeof(ActionInputOutgoingRequest), typeDiscriminator: "outgoingRequest")]
[JsonDerivedType(typeof(ActionInputDIDCommTrustPing), typeDiscriminator: "didcommTrustPing")]
[JsonDerivedType(typeof(ActionInputDIDCommMessage), typeDiscriminator: "didcommMessage")]

[JsonDerivedType(typeof(ActionInputW3cCredential), typeDiscriminator: "w3cCredential")]
[JsonDerivedType(typeof(ActionInputW3cSdCredential), typeDiscriminator: "w3cSdCredential")]
[JsonDerivedType(typeof(ActionInputAnoncredCredential), typeDiscriminator: "anoncredCredential")]

[JsonDerivedType(typeof(ActionInputVerifyW3cCredential), typeDiscriminator: "verifyW3cCredential")]
[JsonDerivedType(typeof(ActionInputVerifyW3cSdCredential), typeDiscriminator: "verifyW3cSdCredential")]
[JsonDerivedType(typeof(ActionInputVerifyAnoncredCredential), typeDiscriminator: "verifyAnoncredCredential")]
public abstract class ActionInput
{
    [JsonPropertyName("id")] 
    public Guid Id { get; set; }
}