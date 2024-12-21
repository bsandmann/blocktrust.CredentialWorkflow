namespace Blocktrust.VerifiableCredential.VC;

using Blocktrust.VerifiableCredential.Common;

public record CredentialSubject
{
    /// <summary>
    /// Optional
    /// https://www.w3.org/TR/vc-data-model/#identifiers
    /// </summary>
    public Uri? Id { get; init; } 
    
    // /// <summary>
    // /// Matches the <see cref="Id"/>
    // /// Optional link to reference an exisiting DID Dataset
    // /// Not part of any Equality comparision of Json Serialization
    // /// </summary>
    public Guid? DidDatatsetVerifiableCredentialSubjectId { get; set; }
    
    /// <summary>
    /// Matches the <see cref="Id"/>
    /// in case the Id is a valid URI pointing to a website
    /// to represent the Credential Issuer instead of a DID
    /// Not part of any Equality comparision of Json Serialization
    /// </summary>
    public int? IdentityUriVerifiableCredentialSubjectId { get; set; }
    
    public IDictionary<string, object>? AdditionalData { get; init; }
    
    public SerializationOption? SerializationOption { get; init; }
    
     
    public virtual bool Equals(CredentialSubject? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return EqualityComparer<Uri?>.Default.Equals(Id, other.Id) &&
               DictionaryStringObjectJsonEquals.JsonEquals(AdditionalData, other.AdditionalData) &&
               EqualityComparer<SerializationOption?>.Default.Equals(SerializationOption, other.SerializationOption);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Id);
        DictionaryStringObjectJsonEquals.AddToHashCode(AdditionalData, ref hashCode);
        hashCode.Add(SerializationOption);
        return hashCode.ToHashCode();
    }
}