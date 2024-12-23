namespace Blocktrust.CredentialBadges.OpenBadges;

using System;
using System.Text.Json.Serialization;

/// <summary>
/// The information in CredentialStatus is used to discover information about the current status 
/// of a verifiable credential, such as whether it is suspended or revoked.
/// <see cref="https://www.imsglobal.org/spec/ob/v3p0/#credentialstatus"/>
/// </summary>
public class CredentialStatus
{
    /// <summary>
    /// The value MUST be the URL of the issuer's credential status method. [1]
    /// </summary>
    [JsonPropertyName("id")]
    public required Uri Id { get; set; }

    /// <summary>
    /// The name of the credential status method. [1]
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    /// <summary>
    /// This property is used by Identus for revocation
    /// TODO evaluation is this is all according to the spec and also used by OpenBadges
    /// </summary>
    [JsonPropertyName("statusPurpose")]
    public string? StatusPurpose { get; set; }
    
    /// <summary>
    /// This property is used by Identus for revocation
    /// TODO evaluation is this is all according to the spec and also used by OpenBadges
    /// </summary> 
    [JsonPropertyName("statusListIndex")]
    public int? StatusListIndex { get; set; }
    
    /// <summary>
    /// This property is used by Identus for revocation
    /// TODO evaluation is this is all according to the spec and also used by OpenBadges
    /// </summary> 
    [JsonPropertyName("statusListCredential")]
    public string? StatusListCredential { get; set; }
}