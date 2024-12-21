namespace Blocktrust.VerifiableCredential;

using System.Text.Json;
using System.Text.Json.Serialization;
using Common;
using Common.Converters;
using Common.JwtModels;
using VC;

public record VerifiableCredential
{
    /// <summary>
    /// The unique ID of the Verifiable Credential of the Database
    /// If not set, this is a new Verifiable Credential which is not written to the database yet
    /// Not part of any Equality Comparision or Json Serialization
    /// </summary>
    [JsonIgnore]
    public Guid? VerifiableCredentialEntityId { get; init; }
    
    
    // /// <summary>
    // /// Enum to describe the data model type (1.1 or 2.0)
    // /// </summary>
    [JsonIgnore] public EDataModelType DataModelType { get; set; }

    /// <summary>
    /// https://www.w3.org/TR/vc-data-model/#contexts
    /// Not optional to be a valid VC
    /// </summary>
    [JsonPropertyName("@context")]
    [JsonConverter(typeof(ContextConverter))]
    public required CredentialOrPresentationContext CredentialContext { get; init; }

    /// <summary>
    /// Optional
    /// https://www.w3.org/TR/vc-data-model/#identifiers
    /// </summary>
    [JsonPropertyName("id")]
    public Uri? Id { get; init; }
    
    /// <summary>
    /// Matches the <see cref="Id"/>
    /// in case the Id is a valid URI pointing to a website
    /// to represent the Credential id
    /// </summary>
    [JsonIgnore]
    public int? IdentityUriVerifiableCredentialId { get; set; }
   
    /// <summary>
    /// Linking the VerifiableCredential to the DidDataset for the Credential Id
    /// in case it is a DID
    /// </summary>
    [JsonIgnore]
    public Guid? DidDatasetVerifiableCredentialId { get; set; }

    /// <summary>
    /// https://www.w3.org/TR/vc-data-model/#dfn-type
    /// </summary>
    [JsonPropertyName("type")]
    [JsonConverter(typeof(TypeConverter))]
    public required CredentialOrPresentationType Type { get; init; }

    /// <summary>
    /// Issuer
    /// This should actually be "required", but when parsing a JWT, this value can be omitted and
    /// then later added from the claims of the JWT. To make parsing possible, this is declared optional here.
    /// </summary>
    [JsonPropertyName("issuer")]
    [JsonConverter(typeof(VcIssuerConverter))]
    public CredentialIssuer? CredentialIssuer { get; init; }

    /// <summary>
    /// Required for Data Model 1.1
    /// Not existing in Data Model 2.0
    /// </summary>
    [JsonPropertyName("issuanceDate")]
    public DateTime? IssuanceDate { get; init; }

    /// <summary>
    /// Optional Property for Data Model 1.1
    /// Not existing in Data Model 2.0
    /// </summary>
    [JsonPropertyName("expirationDate")]
    public DateTime? ExpirationDate { get; init; }

    /// <summary>
    /// Optional Property for Data Model 2.0
    /// Not existing in Data Model 1.1
    /// </summary>
    [JsonPropertyName("validFrom")]
    public DateTime? ValidFrom { get; init; }

    /// <summary>
    /// Optional Property for Data Model 2.0
    /// Not existing in Data Model 1.1
    /// </summary>
    [JsonPropertyName("validUntil")]
    public DateTime? ValidUntil { get; init; }

    /// <summary>
    /// Non optional property for Data Model 1.1 and 2.0
    /// </summary>
    [JsonPropertyName("credentialSubject")]
    [JsonConverter(typeof(VcCredentialSubjectConverter))]
    public required List<CredentialSubject> CredentialSubjects { get; init; }

    /// <summary>
    /// https://www.w3.org/TR/vc-data-model/#proofs-signatures
    /// At least one proof mechanism, and the details necessary to evaluate that proof,
    /// MUST be expressed for a credential or presentation to be a verifiable credential
    /// or verifiable presentation; that is, to be verifiable. 
    /// </summary>
    [JsonPropertyName("proof")]
    [JsonConverter(typeof(VcCredentialOrPresentationProofConverter))]
    public List<CredentialOrPresentationProof>? Proofs { get; init; }

    /// <summary>
    /// https://www.w3.org/TR/vc-data-model/#status
    /// Optional Status for Data Model 1.1 and 2.0
    /// </summary>
    [JsonPropertyName("credentialStatus")]
    public CredentialStatus? CredentialStatus { get; init; }

    /// <summary>
    /// Optional definition of Credential Schema for Data Model 1.1 and 2.0
    /// https://www.w3.org/TR/vc-data-model/#data-schemas
    /// </summary>
    [JsonPropertyName("credentialSchema")]
    [JsonConverter(typeof(VcCredentialSchemaConverter))]
    public List<CredentialSchema>? CredentialSchemas { get; init; }

    /// <summary>
    /// Optional definition of one or more Refresh Service for Data Model 1.1 and 2.0
    /// https://www.w3.org/TR/vc-data-model/#refreshing
    /// </summary>
    [JsonPropertyName("refreshService")]
    [JsonConverter(typeof(VcRefreshServiceConverter))]
    public List<CredentialRefreshService>? RefreshServices { get; init; }

    /// <summary>
    /// Optional definition of Terms of Use for Data Model 1.1 and 2.0
    ///https://www.w3.org/TR/vc-data-model/#terms-of-use
    /// </summary>
    [JsonPropertyName("termsOfUse")]
    [JsonConverter(typeof(VcTermsOfUseConverter))]
    public List<CredentialOrPresentationTermsOfUse>? TermsOfUses { get; init; }

    /// <summary>
    /// Optional definition of Terms of Use for Data Model 1.1 and 2.0
    ///https://www.w3.org/TR/vc-data-model/#terms-of-use
    /// </summary>
    [JsonPropertyName("evidence")]
    [JsonConverter(typeof(VcEvidenceConverter))]
    public List<CredentialEvidence>? Evidences { get; init; }

    [JsonExtensionData] public IDictionary<string, object>? AdditionalData { get; init; }

    /// <summary>
    /// Artefact of the JWT parsing process. If a JWT is parsed, this property contains all
    /// the information not capured in the VerifiableCredential record.
    /// This value is optional, and doesn't need
    /// to be included
    /// </summary>
    [JsonIgnore]
    public JwtParsingArtefact? JwtParsingArtefact { get; set; }
   
    /// <summary>
    /// Options for serialization to force a single VerifiableCredential inside of a
    /// Presentation to be written as an array or not.
    /// </summary>
    [JsonIgnore]
    public SerializationOption? SerializationOption { get; init; }
    
    /// <summary>
    ///  The semantic hash of the Credential. A credential with the same semantic hash is considered the same credential.
    ///  It might even be different in structure (other ordering) but it is considered the same credential.
    /// </summary>
    [JsonIgnore]
    public string SemanticHash { get; set; }
    


    public virtual bool Equals(VerifiableCredential? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return DataModelType == other.DataModelType &&
               CredentialContext.Equals(other.CredentialContext) &&
               Equals(Id, other.Id) &&
               Type.Equals(other.Type) &&
               CredentialIssuer == other.CredentialIssuer &&
               IssuanceDate == other.IssuanceDate &&
               ExpirationDate == other.ExpirationDate &&
               ValidFrom == other.ValidFrom &&
               ValidUntil == other.ValidUntil &&
               ListsEqual(other.CredentialSubjects,other.CredentialSubjects) &&
               ListsEqual(Proofs, other.Proofs) &&
               Equals(CredentialStatus, other.CredentialStatus) &&
               ListsEqual(CredentialSchemas, other.CredentialSchemas) &&
               ListsEqual(RefreshServices, other.RefreshServices) &&
               ListsEqual(TermsOfUses, other.TermsOfUses) &&
               ListsEqual(Evidences, other.Evidences) &&
               Equals(JwtParsingArtefact, other.JwtParsingArtefact) &&
               DictionaryStringObjectJsonEquals.JsonEquals(AdditionalData, other.AdditionalData) &&
               Equals(SerializationOption, other.SerializationOption);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(DataModelType);
        hashCode.Add(CredentialContext);
        hashCode.Add(Id);
        hashCode.Add(Type);
        hashCode.Add(CredentialIssuer);
        hashCode.Add(IssuanceDate);
        hashCode.Add(ExpirationDate);
        hashCode.Add(ValidFrom);
        hashCode.Add(ValidUntil);
        foreach (var item in CredentialSubjects)
        {
            hashCode.Add(item);
        }

        AddListToHashCode(Proofs, ref hashCode);
        hashCode.Add(CredentialStatus);
        AddListToHashCode(CredentialSchemas, ref hashCode);
        AddListToHashCode(RefreshServices, ref hashCode);
        AddListToHashCode(TermsOfUses, ref hashCode);
        AddListToHashCode(Evidences, ref hashCode);
        hashCode.Add(JwtParsingArtefact);
        DictionaryStringObjectJsonEquals.AddToHashCode(AdditionalData, ref hashCode);
        hashCode.Add(SerializationOption);
        return hashCode.ToHashCode();
    }

    private static bool ListsEqual<T>(List<T>? list1, List<T>? list2)
    {
        if (list1 == null && list2 == null) return true;
        if (list1 == null || list2 == null) return false;
        return list1.SequenceEqual(list2);
    }

    private static void AddListToHashCode<T>(List<T>? list, ref HashCode hashCode)
    {
        if (list != null)
        {
            foreach (var item in list)
            {
                hashCode.Add(item);
            }
        }
    }
    
    private static readonly JsonSerializerOptions JsonSerializationOptions = CreateJsonSerializerOptions();
    private static readonly JsonSerializerOptions JsonSerializationOptionsWithIndent = CreateJsonSerializerOptions(true);

    private static JsonSerializerOptions CreateJsonSerializerOptions(bool indented = false)
    {
        return new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = indented
        };
    }
    
    public string ToJson(bool indented = false) => JsonSerializer.Serialize(this, indented ? JsonSerializationOptionsWithIndent : JsonSerializationOptions);
  
}