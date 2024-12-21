namespace Blocktrust.VerifiableCredential.VC;
using Common;

public record CredentialOrPresentationType
{
    /// <summary>
    /// TODO A hashset does not guarantee order, but it should be align with the
    /// Ids we have in CredentialOrPresentationTypeId table.
    /// The current datastructure is not adequate and should be changed to a dircionary
    /// of <string,int?>
    /// </summary>
    public required HashSet<string> Type { get; init; }

    /// <summary>
    /// List of the EntityIds of the CredentialOrPresentationType
    /// </summary>
    public IList<int>? CredentialOrPresentationTypeIds { get; init; }
    
    public SerializationOption? SerializationOption { get; init; }
    
    public virtual bool Equals(CredentialOrPresentationType? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Type.SetEquals(other.Type) &&
               EqualityComparer<SerializationOption?>.Default.Equals(SerializationOption, other.SerializationOption);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var type in Type)
        {
            hash.Add(type);
        }
        hash.Add(SerializationOption);
        return hash.ToHashCode();
    }
}