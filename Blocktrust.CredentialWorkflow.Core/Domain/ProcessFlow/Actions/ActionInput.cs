using System.Text.Json.Serialization;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.DID;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Issuance;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Outgoing;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Verification;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Validation;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.JWT;

namespace Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]

// Issuance
[JsonDerivedType(typeof(IssueW3cCredential), typeDiscriminator: "issueW3cCredential")]
[JsonDerivedType(typeof(IssueW3CSdCredential), typeDiscriminator: "w3cSdCredential")]
[JsonDerivedType(typeof(IssueAnoncredCredential), typeDiscriminator: "anoncredCredential")]

// Verification
[JsonDerivedType(typeof(VerifyW3cCredential), typeDiscriminator: "verifyW3cCredential")]
[JsonDerivedType(typeof(VerifyW3cSdCredential), typeDiscriminator: "verifyW3cSdCredential")]
[JsonDerivedType(typeof(VerifyAnoncredCredential), typeDiscriminator: "verifyAnoncredCredential")]

// Communication
[JsonDerivedType(typeof(DIDCommAction), typeDiscriminator: "didComm")]
[JsonDerivedType(typeof(HttpAction), typeDiscriminator: "http")]
[JsonDerivedType(typeof(EmailAction), typeDiscriminator: "email")]

// Validation
[JsonDerivedType(typeof(W3cValidationAction), typeDiscriminator: "w3cValidation")]
[JsonDerivedType(typeof(CustomValidationAction), typeDiscriminator: "customValidation")]

// DID Management
[JsonDerivedType(typeof(CreateDIDAction), typeDiscriminator: "createDID")]
[JsonDerivedType(typeof(UpdateDIDAction), typeDiscriminator: "updateDID")]
[JsonDerivedType(typeof(DeleteDIDAction), typeDiscriminator: "deleteDID")]

// JWT
[JsonDerivedType(typeof(JwtTokenGeneratorAction), typeDiscriminator: "jwtTokenGenerator")]

public abstract class ActionInput
{
    [JsonPropertyName("id")] 
    public Guid Id { get; set; }
}