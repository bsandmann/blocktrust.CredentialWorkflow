namespace Blocktrust.VerifiableCredential.VC;

using Common;

public record CredentialIssuer
{
    public CredentialIssuer(Uri issuerId, string? issuerName = null, string? issuerDescription = null, int? identityUriVerifiableCredentialIssuerId = null, Guid? didDatasetVerifiableCredentialIssuerId = null, IDictionary<string, object>? additionalData = null)
    {
        IssuerId = issuerId;
        IssuerName = issuerName;
        IssuerDescription = issuerDescription;
        DidDatasetVerifiableCredentialIssuerId = didDatasetVerifiableCredentialIssuerId;
        IdentityUriVerifiableCredentialIssuerId = identityUriVerifiableCredentialIssuerId;
        AdditionalData = additionalData;
    }

    public CredentialIssuer(Uri issuerId, IDictionary<string, LanguageModel>? issuerNameLanguages, IDictionary<string, LanguageModel>? issuerDescriptionLanguages, int? identityUriVerifiableCredentialIssuerId = null, Guid? didDatasetVerifiableCredentialIssuerId = null,
        IDictionary<string, object>? additionalData = null)
    {
        IssuerId = issuerId;
        IssuerNameLanguages = issuerNameLanguages;
        IssuerDescriptionLanguages = issuerDescriptionLanguages;
        DidDatasetVerifiableCredentialIssuerId = didDatasetVerifiableCredentialIssuerId;
        IdentityUriVerifiableCredentialIssuerId = identityUriVerifiableCredentialIssuerId;
        AdditionalData = additionalData;
    }

    /// <summary>
    /// The Id of the issuer
    /// https://www.w3.org/TR/vc-data-model/#issuer 
    /// </summary>
    public Uri IssuerId { get; }

    /// <summary>
    /// Matches the <see cref="Id"/>
    /// in case the Id is a valid URI pointing to a website
    /// to represent the Credential Issuerid
    /// </summary>
    public int? IdentityUriVerifiableCredentialIssuerId { get; set; }

    /// <summary>
    /// Linking the VerifiableCredential to the DidDataset for the Credential IssuerId
    /// in case it is a DID
    /// </summary>
    public Guid? DidDatasetVerifiableCredentialIssuerId { get; set; }

    /// <summary>
    /// Optional IssuerName
    /// https://www.w3.org/TR/vc-data-model/#issuer
    /// </summary>
    public string? IssuerName { get; }

    /// <summary>
    /// Alternative implementatino of Dictionary of issuer names in different languages
    /// https://w3c.github.io/vc-data-model/#issuer
    /// eg 'fr': 'Le gouvernement du Québec'
    /// </summary>
    public IDictionary<string, LanguageModel>? IssuerNameLanguages { get; }

    /// <summary>
    /// Optional Issuer Description
    /// https://www.w3.org/TR/vc-data-model/#issuer
    /// </summary>
    public string? IssuerDescription { get; }

    /// <summary>
    /// Alternative implementatino of Dictionary of issuer descriptions in different languages
    /// https://w3c.github.io/vc-data-model/#issuer
    /// e.g 'fr': 'Le gouvernement du Québec'
    /// </summary>
    public IDictionary<string, LanguageModel>? IssuerDescriptionLanguages { get; }

    /// <summary>
    /// Additional data
    /// </summary>
    public IDictionary<string, object>? AdditionalData { get; init; }

    public virtual bool Equals(CredentialIssuer? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return IssuerId.Equals(other.IssuerId) &&
               IssuerName == other.IssuerName &&
               IssuerDescription == other.IssuerDescription &&
               DictionaryStringObjectJsonEquals.JsonEquals(IssuerNameLanguages, other.IssuerNameLanguages) &&
               DictionaryStringObjectJsonEquals.JsonEquals(IssuerDescriptionLanguages, other.IssuerDescriptionLanguages) &&
               DictionaryStringObjectJsonEquals.JsonEquals(AdditionalData, other.AdditionalData);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(IssuerId);
        hashCode.Add(IssuerName);
        hashCode.Add(IssuerDescription);
        DictionaryStringObjectJsonEquals.AddToHashCode(IssuerNameLanguages, ref hashCode);
        DictionaryStringObjectJsonEquals.AddToHashCode(IssuerDescriptionLanguages, ref hashCode);
        DictionaryStringObjectJsonEquals.AddToHashCode(AdditionalData, ref hashCode);
        return hashCode.ToHashCode();
    }
}