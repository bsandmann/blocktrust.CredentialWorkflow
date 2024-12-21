namespace Blocktrust.VerifiableCredential.VC;

using System.Text.Json.Serialization;
using Blocktrust.VerifiableCredential.Common;

public record CredentialOrPresentationProof
{
    /// <summary>
    /// https://www.w3.org/TR/vc-data-model/#proofs-signatures
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; init; } 
    
    [JsonExtensionData]
    public IDictionary<string, object>? AdditionalData { get; init; }
    
    public SerializationOption? SerializationOption { get; init; }
    
    public virtual bool Equals(CredentialOrPresentationProof? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return Type == other.Type &&
               DictionaryStringObjectJsonEquals.JsonEquals(AdditionalData, other.AdditionalData) &&
               EqualityComparer<SerializationOption?>.Default.Equals(SerializationOption, other.SerializationOption);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Type);
        DictionaryStringObjectJsonEquals.AddToHashCode(AdditionalData, ref hashCode);
        hashCode.Add(SerializationOption);
        return hashCode.ToHashCode();
    }
}