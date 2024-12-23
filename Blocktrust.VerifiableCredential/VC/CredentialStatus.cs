namespace Blocktrust.VerifiableCredential.VC;

using System.Text.Json.Serialization;
using Common;

/// <summary>
/// https://www.w3.org/TR/vc-data-model/#status
/// </summary>
public record CredentialStatus
{
    /// <summary>
    /// Required
    /// </summary>
    [JsonPropertyName("id")]
    public required Uri Id { get; init; } 
   
    /// <summary>
    /// Required
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; init; }
    
    [JsonExtensionData]
    public IDictionary<string, object>? AdditionalData { get; init; }
    
    public virtual bool Equals(CredentialStatus? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return Id.Equals(other.Id) &&
               Type == other.Type &&
               DictionaryStringObjectJsonEquals.JsonEquals(AdditionalData, other.AdditionalData);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Id);
        hashCode.Add(Type);
        DictionaryStringObjectJsonEquals.AddToHashCode(AdditionalData, ref hashCode);
        return hashCode.ToHashCode();
    }
}