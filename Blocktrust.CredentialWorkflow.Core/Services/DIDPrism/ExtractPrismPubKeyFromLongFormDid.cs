using Blocktrust.CredentialWorkflow.Core.Prism;
using OpenPrismNode;
using KeyUsage = OpenPrismNode.KeyUsage;
using PublicKey = OpenPrismNode.PublicKey;

namespace Blocktrust.CredentialWorkflow.Core.Services.DIDPrism;

public class ExtractPrismPubKeyFromLongFormDid
{
    public byte[] Extract(string did)
    {
        if (did.Split(':').Length <= 3)
        {
            throw new ArgumentException("Short-form DID resolution is not implemented");
        }

        string encodedPKeys = did.Split(":")[3];
        var bytes = PrismEncoding.Base64ToByteArray(encodedPKeys);
        var atalaOperation = AtalaOperation.Parser.ParseFrom(bytes);

        var issuingKey = atalaOperation.CreateDid.DidData.PublicKeys.First(p => p.Usage == KeyUsage.IssuingKey);

        var prismPublicKey = new PrismPublicKey(
            keyUsage: Enum.Parse<PrismKeyUsage>(issuingKey.Usage.ToString()),
            keyId: issuingKey.Id,
            curve: issuingKey.EcKeyData is not null ? issuingKey.EcKeyData.Curve : issuingKey.CompressedEcKeyData.Curve,
            keyX: issuingKey.KeyDataCase == PublicKey.KeyDataOneofCase.EcKeyData
                ? issuingKey.EcKeyData?.X.ToByteArray()
                : issuingKey.KeyDataCase == PublicKey.KeyDataOneofCase.CompressedEcKeyData
                    ? PrismPublicKey
                        .Decompress(PrismEncoding.ByteStringToByteArray(issuingKey.CompressedEcKeyData.Data),
                            issuingKey.CompressedEcKeyData.Curve).Value.Item1
                    : null,
            keyY: issuingKey.KeyDataCase == PublicKey.KeyDataOneofCase.EcKeyData
                ? issuingKey.EcKeyData?.Y.ToByteArray()
                : issuingKey.KeyDataCase == PublicKey.KeyDataOneofCase.CompressedEcKeyData
                    ? PrismPublicKey
                        .Decompress(PrismEncoding.ByteStringToByteArray(issuingKey.CompressedEcKeyData.Data),
                            issuingKey.CompressedEcKeyData.Curve).Value.Item2
                    : null);

        return prismPublicKey.LongByteArray;
    }
}