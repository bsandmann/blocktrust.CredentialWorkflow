namespace Blocktrust.CredentialWorkflow.Core.Entities.DIDComm;

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

public class PeerDIDSecretEntity
{
    //TODO is a oversimplicfication, since i should save the kid then add multiple VerifactionMaterialEntties to that kid
    //Also I guess there will be multiple other entries related to that did (like also the invitation) or logs

    /// <summary>
    /// The Id as Guid
    /// </summary>
    public Guid PeerDIDSecretId { get; set; }

    /// <summary>
    /// The key-id-of the secret
    /// </summary>
    [Unicode(true)]
    [MaxLength(1000)]
    public required string Kid { get; set; }

    /// <summary>
    /// The MethodType enum as int (see VerificationMethodType)
    /// </summary>
    public int VerificationMethodType { get; set; }

    /// <summary>
    /// The format enum as int (see VerificationMaterialFormat)
    /// </summary>
    public int VerificationMaterialFormat { get; set; }

    /// <summary>
    /// The value of the secret: Depending on the format this could be eg. a Base64 encoded string or a JWK 
    /// </summary>
    [Unicode(true)]
    [MaxLength(2000)]
    public required string Value { get; set; }

    /// <summary>
    /// Date of creation
    /// </summary>
    public DateTime CreatedUtc { get; set; }
}