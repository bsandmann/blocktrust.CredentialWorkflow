namespace Blocktrust.CredentialWorkflow.Core.Domain.Credential;

public record Credential : VerifiableCredential.VerifiableCredential
{
    // JWT specific fields that will be populated by the parser
    public string? HeaderJson { get; set; }
    public string? PayloadJson { get; set; }

    // Constructors
    public Credential()
    {
    }

    // Constructor that takes base class
    public Credential(VerifiableCredential.VerifiableCredential baseCredential)
    {
        // Copy all properties from base credential
        CredentialContext = baseCredential.CredentialContext;
        Type = baseCredential.Type;
        CredentialSubjects = baseCredential.CredentialSubjects;
        CredentialIssuer = baseCredential.CredentialIssuer;
        Id = baseCredential.Id;
        IssuanceDate = baseCredential.IssuanceDate;
        ExpirationDate = baseCredential.ExpirationDate;
        ValidFrom = baseCredential.ValidFrom;
        ValidUntil = baseCredential.ValidUntil;
        Proofs = baseCredential.Proofs;
        CredentialStatus = baseCredential.CredentialStatus;
        CredentialSchemas = baseCredential.CredentialSchemas;
        RefreshServices = baseCredential.RefreshServices;
        TermsOfUses = baseCredential.TermsOfUses;
        Evidences = baseCredential.Evidences;
        AdditionalData = baseCredential.AdditionalData;
        JwtParsingArtefact = baseCredential.JwtParsingArtefact;
        SerializationOption = baseCredential.SerializationOption;
        // Note: copying read-only fields where needed
        DataModelType = baseCredential.DataModelType;
    }
}