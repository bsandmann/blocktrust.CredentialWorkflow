namespace Blocktrust.VerifiableCredential.Common;

public record SerializationOption
{
    public required bool UseArrayEvenForSingleElement { get; init; }
}