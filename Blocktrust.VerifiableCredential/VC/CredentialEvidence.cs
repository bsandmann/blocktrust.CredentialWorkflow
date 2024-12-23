namespace Blocktrust.VerifiableCredential.VC;

using System.Text.Json.Serialization;
using Blocktrust.VerifiableCredential.Common;

/// <summary>
/// https://www.w3.org/TR/vc-data-model/#evidence
/// </summary>
public record CredentialEvidence
{
    /// <summary>
    /// Optional
    /// </summary>
    [JsonPropertyName("id")]
    public Uri? Id { get; init; } 
    
    /// <summary>
    /// Required
    /// </summary>
    [JsonPropertyName("type")]
    public required IList<string> Type { get; init; }
    
    public IDictionary<string, object>? AdditionalData { get; init; } 
    
    public SerializationOption? SerializationOption { get; init; }
    
    public virtual bool Equals(CredentialEvidence? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return Equals(Id, other.Id) &&
               Type.SequenceEqual(other.Type) &&
               DictionaryStringObjectJsonEquals.JsonEquals(AdditionalData, other.AdditionalData) &&
               EqualityComparer<SerializationOption?>.Default.Equals(SerializationOption, other.SerializationOption);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Id);
        foreach (var type in Type)
        {
            hashCode.Add(type);
        }
        DictionaryStringObjectJsonEquals.AddToHashCode(AdditionalData, ref hashCode);
        hashCode.Add(SerializationOption);
        return hashCode.ToHashCode();
    }

   
}