namespace Blocktrust.VerifiableCredential.VC;

using System.Text.Json.Serialization;
using Blocktrust.VerifiableCredential.Common;

/// <summary>
/// https://www.w3.org/TR/vc-data-model/#refreshing
/// </summary>
public record CredentialRefreshService
{
    /// <summary>
    /// Require
    /// </summary>
    [JsonPropertyName("id")]
    public required Uri Id { get; init; } 
    
    /// <summary>
    /// Required
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; init; }
    
    public IDictionary<string, object>? AdditionalData { get; init; } 
    
    public SerializationOption? SerializationOption { get; init; }
    
    public virtual bool Equals(CredentialRefreshService? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return Id.Equals(other.Id) &&
               Type == other.Type &&
               DictionaryStringObjectJsonEquals.JsonEquals(AdditionalData, other.AdditionalData) &&
               EqualityComparer<SerializationOption?>.Default.Equals(SerializationOption, other.SerializationOption);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Id);
        hashCode.Add(Type);
        DictionaryStringObjectJsonEquals.AddToHashCode(AdditionalData, ref hashCode);
        hashCode.Add(SerializationOption);
        return hashCode.ToHashCode();
    }
}