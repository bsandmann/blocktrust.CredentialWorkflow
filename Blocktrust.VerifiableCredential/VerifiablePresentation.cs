namespace Blocktrust.VerifiableCredential;

using System.Text.Json;
using System.Text.Json.Serialization;
using Common;
using Common.Converters;
using Common.JwtModels;
using VC;

/// <summary>
/// https://www.w3.org/TR/vc-data-model/#presentations-0
/// </summary>
public record VerifiablePresentation
{
    /// <summary>
    /// The unique ID of the Verifiable Presentation in the Database
    /// If not set, this is a new Verifiable Presentation which is not written to the database yet
    /// Not part of any Equality Comparision or Json Serialization
    /// </summary>
    [JsonIgnore]
    public Guid? VerifiablePresentationEntityId { get; init; }
    
    // /// <summary>
    // /// Enum to describe the data model type (1.1 or 2.0)
    // /// </summary>
    [JsonIgnore]
    public EDataModelType DataModelType { get; set; }
    
    /// <summary>
    /// https://www.w3.org/TR/vc-data-model/#contexts
    /// Not optional to be a valid VC/C
    /// </summary>
    [JsonPropertyName("@context")]
    [JsonConverter(typeof(ContextConverter))]
    public required CredentialOrPresentationContext PresentationContext { get; init; }
    
    /// <summary>
    /// Optional
    /// https://www.w3.org/TR/vc-data-model/#identifiers
    /// https://www.w3.org/TR/vc-data-model/#presentations-0
    /// </summary>
    [JsonPropertyName("id")]
    public Uri? Id { get; init; }
   
    /// <summary>
    /// Matches the <see cref="Id"/>
    /// in case the Id is a valid URI pointing to a website
    /// to represent the Credential id
    /// </summary>
    [JsonIgnore]
    public int? IdentityUriVerifiablePresentationId { get; set; }
    
    /// <summary>
    /// Matches the Id of the <see cref="VerifiablePresentation"/>
    /// Optional link to reference an exisiting DID Dataset
    /// Not part of any Equality comparision of Json Serialization
    /// </summary>
    [JsonIgnore]
    public Guid? DidDatasetVerifiablePresentationId { get; set; }
    
    /// <summary>
    /// https://www.w3.org/TR/vc-data-model/#dfn-type
    /// https://www.w3.org/TR/vc-data-model/#presentations-0 
    /// </summary>
    [JsonPropertyName("type")]
    [JsonConverter(typeof(TypeConverter))]
    public required CredentialOrPresentationType Type { get; init; }
    
    /// <summary>
    /// Not Required in VC Data Model 2.0
    /// </summary>
    [JsonPropertyName("verifiableCredential")]
    [JsonConverter(typeof(VerifiableCredentialListConverter))]
    public  List<VerifiableCredential>? VerifiableCredentials { get; init; }
    
    /// <summary>
    /// Optional Holder (Entity that is generating the presentation)
    /// https://www.w3.org/TR/vc-data-model/#presentations-0
    /// </summary>
    [JsonPropertyName("holder")]
    public Uri? Holder { get; init; }
    
    /// <summary>
    /// Matches the <see cref="Holder"/>
    /// Optional link to reference an exisiting DID Dataset
    /// Not part of any Equality comparision of Json Serialization
    /// </summary>
    [JsonIgnore]
    public Guid? DidDatasetVerifiablePresentationHolderId { get; set; }
    
    /// <summary>
    /// Matches the <see cref="Id"/>
    /// in case the Id is a valid URI pointing to a website
    /// to represent the Presentation holder
    /// </summary>
    [JsonIgnore]
    public int? IdentityUriVerifiablePresentationHolderId { get; set; }
    
    /// <summary>
    /// https://www.w3.org/TR/vc-data-model/#proofs-signatures
    /// https://www.w3.org/TR/vc-data-model/#presentations-0
    /// </summary>
    [JsonPropertyName("proof")]
    [JsonConverter(typeof(VcCredentialOrPresentationProofConverter))]
    public List<CredentialOrPresentationProof>? Proofs { get; init; }
    
    /// <summary>
    /// Optional definition of Terms of Use for Data Model 1.1 and 2.0
    ///https://www.w3.org/TR/vc-data-model/#terms-of-use
    /// </summary>
    [JsonPropertyName("termsOfUse")]
    [JsonConverter(typeof(VcTermsOfUseConverter))]
    public List<CredentialOrPresentationTermsOfUse>? TermsOfUses{ get; init; }
    
    [JsonExtensionData] 
    public IDictionary<string, object>? AdditionalData { get; init; }
    
    /// <summary>
    /// Artefact of the JWT parsing process. If a JWT is parsed, this property contains all
    /// the information not capured in the VerifiablePresentation record.
    /// This value is optional, and doesn't need
    /// to be included
    /// </summary>
    [JsonIgnore]
    public JwtParsingArtefact? JwtParsingArtefact { get; set; }
    
    /// <summary>
    ///  The semantic hash of the Presentation. A presentation with the same semantic hash is considered the same presentation.
    ///  It might even be different in structure (other ordering) but it is considered the same Presentation.
    /// </summary>
    [JsonIgnore]
    public string SemanticHash { get; set; }
 
     public virtual bool Equals(VerifiablePresentation? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return DataModelType == other.DataModelType &&
               PresentationContext.Equals(other.PresentationContext) &&
               Equals(Id, other.Id) &&
               Type.Equals(other.Type) &&
               ListsEqual(VerifiableCredentials, other.VerifiableCredentials) &&
               Equals(Holder, other.Holder) &&
               ListsEqual(Proofs, other.Proofs) &&
               ListsEqual(TermsOfUses, other.TermsOfUses) &&
               Equals(JwtParsingArtefact, other.JwtParsingArtefact) &&
               DictionaryStringObjectJsonEquals.JsonEquals(AdditionalData, other.AdditionalData);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(DataModelType);
        hashCode.Add(PresentationContext);
        hashCode.Add(Id);
        hashCode.Add(Type);
        AddListToHashCode(VerifiableCredentials, ref hashCode);
        hashCode.Add(Holder);
        AddListToHashCode(Proofs, ref hashCode);
        AddListToHashCode(TermsOfUses, ref hashCode);
        DictionaryStringObjectJsonEquals.AddToHashCode(AdditionalData, ref hashCode);
        hashCode.Add(JwtParsingArtefact);
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