namespace Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.PresentationWorkflow;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

public class AcceptPresentationRequestAttachmentDataStructure
{
    [JsonPropertyName("iss")]
    [NotNull]
    public string? Iss { get; init; }

    [JsonPropertyName("aud")]
    [NotNull]
    public string? Aud { get; init; }

    [JsonPropertyName("vp")]
    [NotNull]
    public AcceptPresentationRequestAttachmentDataStructureVp? Vp { get; init; }

    [JsonPropertyName("nonce")]
    [NotNull]
    public string? Nonce { get; init; }

    [JsonConstructor]
    public AcceptPresentationRequestAttachmentDataStructure()

    {
    }

    public AcceptPresentationRequestAttachmentDataStructure(string longFormHolderDid, string nonce, string domain, List<string> verifiableCredentials)
    {
        Iss = longFormHolderDid;
        Aud = domain;
        Nonce = nonce;
        Vp = new AcceptPresentationRequestAttachmentDataStructureVp(
            type: new List<string>() { "VerifiablePresentation" },
            context: new List<string>() { "https://www.w3.org/2018/presentations/v1" },
            verifiableCredentials: verifiableCredentials);
    }
}