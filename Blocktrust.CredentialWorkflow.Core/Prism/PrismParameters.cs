namespace Blocktrust.CredentialWorkflow.Core.Prism;

/// <summary>
/// Versioning and protocol parameters according to <see cref="https://github.com/input-output-hk/prism-did-method-spec/blob/main/w3c-spec/PRISM-method.md#versioning-and-protocol-parameters"/>
/// </summary>
public class PrismParameters
{
    public const string Secp256k1CurveName = "secp256k1";
    public const string Ed25519CurveName = "edd25519";
    public const string X25519CurveName = "x25519";
}