namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Action;

using System.Text.Json.Serialization;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(ActionInputCredentialIssuance), typeDiscriminator: "credentialIssuance")]
[JsonDerivedType(typeof(ActionInputCredentialVerification), typeDiscriminator: "credentialVerification")]
[JsonDerivedType(typeof(ActionInputOutgoingRequest), typeDiscriminator: "outgoingRequest")]
public  class ActionInput
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
}