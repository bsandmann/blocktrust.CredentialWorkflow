namespace Blocktrust.VerifiableCredential.VC;

using System.Text.Json;
using Common;

/// <summary>
/// https://www.w3.org/TR/vc-data-model/#contexts
/// </summary>
public sealed record CredentialOrPresentationContext
{
    public List<object>? Contexts { get; init; }
    
    /// <summary>
    /// Optional ids to link to the JsonLdContexts table
    /// </summary>
    public List<int>? JsonLdContextIds { get; init; }
    
    public IDictionary<string, object>? AdditionalData { get; init; }
    
    public SerializationOption? SerializationOption { get; init; }
    
    private static readonly JsonSerializerOptions JsonSerializationOptions = new JsonSerializerOptions();

    public bool Equals(CredentialOrPresentationContext? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return JsonSerializer.Serialize(Contexts, JsonSerializationOptions) == JsonSerializer.Serialize(other.Contexts, JsonSerializationOptions) &&
               DictionaryStringObjectJsonEquals.JsonEquals(AdditionalData, other.AdditionalData) &&
               EqualityComparer<SerializationOption?>.Default.Equals(SerializationOption, other.SerializationOption);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(JsonSerializer.Serialize(Contexts, JsonSerializationOptions));
        hashCode.Add(JsonSerializer.Serialize(AdditionalData, JsonSerializationOptions));
        hashCode.Add(SerializationOption);
        return hashCode.ToHashCode();
    }
}