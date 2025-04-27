using System;
using System.Security.Cryptography;
using System.Text.Json;
using Blocktrust.CredentialWorkflow.Core.Services;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Blocktrust.CredentialWorkflow.Core.Tests.Services.JwtKeyGeneratorServiceTests
{
    public class JwtKeyGeneratorServiceTests
    {
        [Fact]
        public void GenerateRsaKeyPairXml_CreatesValidKeyPair()
        {
            // Act
            var (privateKeyXml, publicKeyXml) = JwtKeyGeneratorService.GenerateRsaKeyPairXml();

            // Assert
            privateKeyXml.Should().NotBeNullOrWhiteSpace();
            publicKeyXml.Should().NotBeNullOrWhiteSpace();
            
            // Private key should be longer than public key (contains more data)
            privateKeyXml.Length.Should().BeGreaterThan(publicKeyXml.Length);
            
            // Both should contain XML structure
            privateKeyXml.Should().Contain("<RSAKeyValue>");
            privateKeyXml.Should().Contain("</RSAKeyValue>");
            publicKeyXml.Should().Contain("<RSAKeyValue>");
            publicKeyXml.Should().Contain("</RSAKeyValue>");
            
            // Private key should contain private parameters that public key doesn't have
            privateKeyXml.Should().Contain("<D>");
            privateKeyXml.Should().Contain("<P>");
            privateKeyXml.Should().Contain("<Q>");
            publicKeyXml.Should().NotContain("<D>");
            publicKeyXml.Should().NotContain("<P>");
            publicKeyXml.Should().NotContain("<Q>");
            
            // Both should have modulus and exponent
            privateKeyXml.Should().Contain("<Modulus>");
            privateKeyXml.Should().Contain("<Exponent>");
            publicKeyXml.Should().Contain("<Modulus>");
            publicKeyXml.Should().Contain("<Exponent>");
        }

        [Fact]
        public void GenerateRsaKeyPairXml_WithCustomKeySize_RespectsSizeParameter()
        {
            // Act
            var (privateKeyXml, publicKeyXml) = JwtKeyGeneratorService.GenerateRsaKeyPairXml(2048);

            // Assert
            privateKeyXml.Should().NotBeNullOrWhiteSpace();
            publicKeyXml.Should().NotBeNullOrWhiteSpace();
            
            // The key length can be validated by importing and checking RSA parameters
            using (var rsa = RSA.Create())
            {
                rsa.FromXmlString(privateKeyXml);
                var parameters = rsa.ExportParameters(true);
                
                // For RSA, key size is determined by modulus length
                int keySize = parameters.Modulus.Length * 8;
                keySize.Should().Be(2048);
            }
        }

        [Fact]
        public void GetAlgorithmIdentifier_ReturnsRsaSha256()
        {
            // Act
            var algorithm = JwtKeyGeneratorService.GetAlgorithmIdentifier();

            // Assert
            algorithm.Should().Be(SecurityAlgorithms.RsaSha256);
        }

        [Fact]
        public void GetRsaPrivateKeyFromXml_WithValidXml_ReturnsSecurityKey()
        {
            // Arrange
            var (privateKeyXml, _) = JwtKeyGeneratorService.GenerateRsaKeyPairXml();

            // Act
            var securityKey = JwtKeyGeneratorService.GetRsaPrivateKeyFromXml(privateKeyXml);

            // Assert
            securityKey.Should().NotBeNull();
            securityKey.Should().BeOfType<RsaSecurityKey>();
        }

        [Fact]
        public void GetRsaPrivateKeyFromXml_WithInvalidXml_ThrowsCryptographicException()
        {
            // Arrange
            var invalidXml = "<RSAKeyValue><InvalidTag>data</InvalidTag></RSAKeyValue>";

            // Act & Assert
            Assert.Throws<CryptographicException>(() => 
                JwtKeyGeneratorService.GetRsaPrivateKeyFromXml(invalidXml));
        }

        [Fact]
        public void GetRsaPrivateKeyFromXml_WithNullInput_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                JwtKeyGeneratorService.GetRsaPrivateKeyFromXml(null));
        }

        [Fact]
        public void GetRsaPublicKeyFromXml_WithValidXml_ReturnsSecurityKey()
        {
            // Arrange
            var (_, publicKeyXml) = JwtKeyGeneratorService.GenerateRsaKeyPairXml();

            // Act
            var securityKey = JwtKeyGeneratorService.GetRsaPublicKeyFromXml(publicKeyXml);

            // Assert
            securityKey.Should().NotBeNull();
            securityKey.Should().BeOfType<RsaSecurityKey>();
        }

        [Fact]
        public void GenerateJwksJsonFromXmlPublicKey_CreatesValidJwksJson()
        {
            // Arrange
            var (_, publicKeyXml) = JwtKeyGeneratorService.GenerateRsaKeyPairXml();
            var keyId = "test-key-id";

            // Act
            var jwksJson = JwtKeyGeneratorService.GenerateJwksJsonFromXmlPublicKey(publicKeyXml, keyId);

            // Assert
            jwksJson.Should().NotBeNullOrWhiteSpace();
            
            // Validate JSON structure
            var jwksDoc = JsonDocument.Parse(jwksJson);
            jwksDoc.RootElement.TryGetProperty("keys", out var keysElement).Should().BeTrue();
            keysElement.ValueKind.Should().Be(JsonValueKind.Array);
            
            // Validate the first key in the array
            var keyElement = keysElement[0];
            keyElement.TryGetProperty("kid", out var kidElement).Should().BeTrue();
            kidElement.GetString().Should().Be(keyId);
            
            keyElement.TryGetProperty("kty", out var ktyElement).Should().BeTrue();
            ktyElement.GetString().Should().Be("RSA");
            
            keyElement.TryGetProperty("alg", out var algElement).Should().BeTrue();
            algElement.GetString().Should().Be(SecurityAlgorithms.RsaSha256);
            
            keyElement.TryGetProperty("n", out var nElement).Should().BeTrue(); // Modulus
            nElement.GetString().Should().NotBeNullOrEmpty();
            
            keyElement.TryGetProperty("e", out var eElement).Should().BeTrue(); // Exponent
            eElement.GetString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void GenerateJwksJsonFromXmlPublicKey_WithEmptyPublicKey_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                JwtKeyGeneratorService.GenerateJwksJsonFromXmlPublicKey(""));
        }
    }
}