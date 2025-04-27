using System.Security.Cryptography;
using System.Xml;
using Microsoft.IdentityModel.Tokens;

namespace Blocktrust.CredentialWorkflow.Core.Services;

using System.Text.Json;

/// <summary>
/// Service for generating and managing JWT security keys
/// </summary>
public static class JwtKeyGeneratorService
{
    // Recommended key size for RSA. 2048 is often minimum, 3072 or 4096 are common.
    private const int RecommendedKeySizeInBits = 3072;

    /// <summary>
    /// Generates a new RSA public/private key pair.
    /// </summary>
    /// <param name="keySizeInBits">The desired key size (e.g., 2048, 3072).</param>
    /// <returns>A tuple containing the Private Key XML string and Public Key XML string.</returns>
    /// <remarks>
    /// The XML format is specific to .NET's RSA implementation.
    /// For broader compatibility, consider PEM or JWK formats in real applications.
    /// </remarks>
    public static (string PrivateKeyXml, string PublicKeyXml) GenerateRsaKeyPairXml(int keySizeInBits = RecommendedKeySizeInBits)
    {
        // Create a new RSA object with the specified key size.
        // The 'using' statement ensures proper disposal of cryptographic resources.
        using (var rsa = RSA.Create(keySizeInBits))
        {
            // Export the private key parameters (includes public parts) to XML format.
            string privateKeyXml = rsa.ToXmlString(true); // 'true' includes private parameters

            // Export only the public key parameters to XML format.
            string publicKeyXml = rsa.ToXmlString(false); // 'false' includes only public parameters

            return (PrivateKeyXml: privateKeyXml, PublicKeyXml: publicKeyXml);
        }
    }

    /// <summary>
    /// Helper to get the algorithm identifier string for RSA SHA-256.
    /// </summary>
    public static string GetAlgorithmIdentifier()
    {
        return SecurityAlgorithms.RsaSha256;
    }

    /// <summary>
    /// Creates an RsaSecurityKey object from the private key XML string.
    /// Used for SIGNING tokens.
    /// </summary>
    /// <param name="privateKeyXml">The XML string containing the private RSA key parameters.</param>
    /// <returns>An RsaSecurityKey object initialized with the private key.</returns>
    /// <exception cref="ArgumentNullException">Thrown if privateKeyXml is null or whitespace.</exception>
    /// <exception cref="CryptographicException">Thrown if the XML string is invalid.</exception>
    public static RsaSecurityKey GetRsaPrivateKeyFromXml(string privateKeyXml)
    {
        if (string.IsNullOrWhiteSpace(privateKeyXml))
        {
            throw new ArgumentNullException(nameof(privateKeyXml), "Private key XML cannot be null or empty.");
        }

        var rsa = RSA.Create();
        try
        {
            // Import the private key parameters from the XML string.
            rsa.FromXmlString(privateKeyXml);

            // Create the RsaSecurityKey object needed by the JWT library.
            // IMPORTANT: Do NOT dispose the 'rsa' object here if RsaSecurityKey needs it later.
            // RsaSecurityKey might take ownership or need it alive depending on implementation details
            // or configuration (like key persistence flags, which aren't used here).
            // For safety with typical usage, create RsaSecurityKey directly.
            // The RSA object loaded via FromXmlString typically doesn't need explicit disposal *if*
            // it's immediately wrapped by RsaSecurityKey and not used elsewhere. However, best
            // practice often involves managing the RSA lifetime carefully if reused.
            // Let's assume RsaSecurityKey manages what it needs internally here.
            return new RsaSecurityKey(rsa);
        }
        catch (Exception ex) when (ex is CryptographicException || ex is XmlException || ex is FormatException)
        {
            // Catch potential exceptions during XML parsing or key import
            throw new CryptographicException("Failed to import RSA private key from XML.", ex);
        }
        // If RsaSecurityKey needed the original RSA object disposed later, would need different pattern.
    }


    /// <summary>
    /// Creates an RsaSecurityKey object from the public key XML string.
    /// Used for VALIDATING tokens.
    /// </summary>
    /// <param name="publicKeyXml">The XML string containing the public RSA key parameters.</param>
    /// <returns>An RsaSecurityKey object initialized with the public key.</returns>
    /// <exception cref="ArgumentNullException">Thrown if publicKeyXml is null or whitespace.</exception>
    /// <exception cref="CryptographicException">Thrown if the XML string is invalid.</exception>
    public static RsaSecurityKey GetRsaPublicKeyFromXml(string publicKeyXml)
    {
        if (string.IsNullOrWhiteSpace(publicKeyXml))
        {
            throw new ArgumentNullException(nameof(publicKeyXml), "Public key XML cannot be null or empty.");
        }

        var rsa = RSA.Create();
        try
        {
            // Import the public key parameters from the XML string.
            rsa.FromXmlString(publicKeyXml);

            // Create the RsaSecurityKey object needed by the JWT library.
            return new RsaSecurityKey(rsa);
        }
        catch (Exception ex) when (ex is CryptographicException || ex is XmlException || ex is FormatException)
        {
            // Catch potential exceptions during XML parsing or key import
            throw new CryptographicException("Failed to import RSA public key from XML.", ex);
        }
    }

    /// <summary>
    /// Generates a JWKS JSON string containing a single RSA public key,
    /// converted from its .NET XML string representation.
    /// </summary>
    /// <param name="publicKeyXml">The RSA public key in .NET XML format (containing Modulus and Exponent).</param>
    /// <param name="keyId">The unique Key ID ('kid') to assign to this key in the JWKS.</param>
    /// <param name="algorithm">Optional: The intended algorithm ('alg'), defaults to RS256.</param>
    /// <returns>A JSON string representing the JsonWebKeySet.</returns>
    /// <exception cref="ArgumentNullException">Thrown if publicKeyXml or keyId are null or whitespace.</exception>
    /// <exception cref="CryptographicException">Thrown if the XML key is invalid or RSA parameters cannot be extracted.</exception>
    /// <exception cref="JsonException">Thrown if JSON serialization fails.</exception>
    public static string GenerateJwksJsonFromXmlPublicKey(
        string publicKeyXml,
        string? keyId = "rsa-key",
        string algorithm = SecurityAlgorithms.RsaSha256
    )
    {
        // --- Input Validation ---
        if (string.IsNullOrWhiteSpace(publicKeyXml))
        {
            throw new ArgumentNullException(nameof(publicKeyXml));
        }

        if (string.IsNullOrWhiteSpace(keyId))
        {
            throw new ArgumentNullException(nameof(keyId), "A Key ID (kid) must be provided.");
        }

        RSAParameters rsaParameters;

        // --- Load RSA parameters from XML ---
        try
        {
            // Create an RSA instance to import the XML key
            using (var rsa = RSA.Create())
            {
                rsa.FromXmlString(publicKeyXml);
                // Export *only* the public parameters
                rsaParameters = rsa.ExportParameters(false); // false = public only
            }
        }
        catch (Exception ex) when (ex is CryptographicException || ex is XmlException || ex is FormatException)
        {
            throw new CryptographicException($"Failed to parse RSA public key XML or export parameters: {ex.Message}", ex);
        }

        // --- Parameter Validation (Modulus and Exponent are required for RSA JWK) ---
        if (rsaParameters.Modulus == null || rsaParameters.Exponent == null)
        {
            throw new CryptographicException("Extracted RSA parameters are missing Modulus or Exponent.");
        }

        // --- Create the JsonWebKey (JWK) ---
        var jwk = new JsonWebKey()
        {
            Kty = JsonWebAlgorithmsKeyTypes.RSA, // Key Type = RSA
            Kid = keyId, // Key ID
            Alg = algorithm, // Algorithm (e.g., "RS256")

            // Modulus (n) - Must be Base64URL encoded
            N = Base64UrlEncoder.Encode(rsaParameters.Modulus),

            // Exponent (e) - Must be Base64URL encoded
            E = Base64UrlEncoder.Encode(rsaParameters.Exponent),
        };

        // --- Create the JsonWebKeySet (JWKS) ---
        // The JWKS is essentially a container object with a "keys" property (an array of JWKs)
        var jwks = new JsonWebKeySet();
        jwks.Keys.Add(jwk); // Add our single key

        // --- Serialize the JWKS to JSON String ---
        try
        {
            // Use System.Text.Json for serialization
            // Use options to make the output indented for readability if desired (optional)
            var options = new JsonSerializerOptions
            {
                WriteIndented = true, // Make it pretty print
                // Ignore null values if any properties were not set (good practice)
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            string jwksJson = JsonSerializer.Serialize(jwks, options);
            return jwksJson;
        }
        catch (JsonException ex)
        {
            // Handle potential serialization errors
            throw new JsonException($"Failed to serialize JWKS object to JSON: {ex.Message}", ex);
        }
    }
}