namespace Blocktrust.CredentialWorkflow.Core.Commands.DIDComm.PresentationWorkflow;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

public class AcceptPresentationRequestAttachmentDataStructureVp
{
    [JsonPropertyName("type")]
    [NotNull]
    public List<string>? Type { get; init; }

    [JsonPropertyName("@context")]
    [NotNull]
    public List<string>? Context { get; init; }

    [JsonPropertyName("verifiableCredential")]
    [NotNull]
    public List<string>? VerifiableCredentials { get; init; }

    [JsonConstructor]
    public AcceptPresentationRequestAttachmentDataStructureVp()

    {
    }

    public AcceptPresentationRequestAttachmentDataStructureVp(List<string> type, List<string> context, List<string> verifiableCredentials)
    {
        Type = type;
        Context = context;
        VerifiableCredentials = verifiableCredentials;
    }
}