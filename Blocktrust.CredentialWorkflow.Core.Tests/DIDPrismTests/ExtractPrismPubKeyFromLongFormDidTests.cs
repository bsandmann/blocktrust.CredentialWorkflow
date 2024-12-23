

using Blocktrust.CredentialWorkflow.Core.Services.DIDPrism;
using FluentAssertions;

namespace Blocktrust.CredentialWorkflow.Core.Tests.DIDPrismTests;

public class ExtractPrismPubKeyFromLongFormDidTests
{
    private readonly ExtractPrismPubKeyFromLongFormDid _extractor;
    private const string ValidLongFormDid = "did:prism:53900b7c1f1c7044ed4989ab46570d2927236e9414febe82ea36aaa917a642dd:CoQBCoEBEkIKDm15LWlzc3Vpbmcta2V5EAJKLgoJc2VjcDI1NmsxEiECfd6iCbzvLCSONelmvs3oS2IYyug8Z3hp9MZeS2W2BrkSOwoHbWFzdGVyMBABSi4KCXNlY3AyNTZrMRIhAkyZEcCAaL-VdPnQOOtulV6DSI6xb1USWExoQlInl2ma";
    private const string ShortFormDid = "did:prism:123456";

    public ExtractPrismPubKeyFromLongFormDidTests()
    {
        _extractor = new ExtractPrismPubKeyFromLongFormDid();
    }

    [Fact]
    public void Extract_WithValidLongFormDid_ShouldReturnPublicKey()
    {
        // Act
        var result = _extractor.Extract(ValidLongFormDid);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Length.Should().Be(65); // Expected length for uncompressed secp256k1 public key (1 byte prefix + 32 bytes X + 32 bytes Y)
        result[0].Should().Be(0x04); // Uncompressed public key prefix
    }

    [Fact]
    public void Extract_WithShortFormDid_ShouldThrowArgumentException()
    {
        // Act
        var action = () => _extractor.Extract(ShortFormDid);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("Short-form DID resolution is not implemented");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalid:did:format")]
    public void Extract_WithInvalidDid_ShouldThrowException(string invalidDid)
    {
        // Act
        var action = () => _extractor.Extract(invalidDid);

        // Assert
        action.Should().Throw<Exception>();
    }

    [Fact]
    public void Extract_WithValidLongFormDid_ShouldReturnCorrectPublicKey()
    {
        // Act
        var result = _extractor.Extract(ValidLongFormDid);

        // Assert
        // First byte should be 0x04 (uncompressed public key marker)
        result[0].Should().Be(0x04);

        // X coordinate from the DID should be: 7ddea209bcef2c248e35e966becde84b6218cae83c677869f4c65e4b65b606b9
        // Y coordinate from the DID should be: 4c9911c08068bf9574f9d038eb6e955e834c8eb16f551216131a109527976996
        
        var expectedXCoordinate = new byte[] {
            0x7d, 0xde, 0xa2, 0x09, 0xbc, 0xef, 0x2c, 0x24, 
            0x8e, 0x35, 0xe9, 0x66, 0xbe, 0xcd, 0xe8, 0x4b,
            0x62, 0x18, 0xca, 0xe8, 0x3c, 0x67, 0x78, 0x69,
            0xf4, 0xc6, 0x5e, 0x4b, 0x65, 0xb6, 0x06, 0xb9
        };

        // Extract X coordinate (bytes 1-32)
        var actualXCoordinate = result.Skip(1).Take(32).ToArray();
        actualXCoordinate.Should().BeEquivalentTo(expectedXCoordinate, 
            "X coordinate should match the one encoded in the DID");
    }
}