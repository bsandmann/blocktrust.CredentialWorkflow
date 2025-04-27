using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Blocktrust.CredentialWorkflow.Core.Services;

/// <summary>
/// Service for generating and managing JWT security keys
/// </summary>
public static class JwtKeyGeneratorService
{
    // Recommendation for HS256 is a key size equal to the hash output size (256 bits = 32 bytes)
    private const int RecommendedKeySizeInBytes = 32;

    /// <summary>
    /// Generates a cryptographically secure random key suitable for HMAC-SHA256,
    /// returned as a Base64 encoded string.
    /// </summary>
    /// <returns>A Base64 encoded string representing the secret key.</returns>
    public static string GenerateHmacSha256SecretKeyString()
    {
        // Create a byte array to hold the key
        byte[] keyBytes = new byte[RecommendedKeySizeInBytes];

        // Fill the byte array with cryptographically strong random bytes
        // using System.Security.Cryptography.RandomNumberGenerator
        using (var randomNumberGenerator = RandomNumberGenerator.Create())
        {
            randomNumberGenerator.GetBytes(keyBytes);
        }

        // Convert the random byte array to a Base64 string for easy storage
        string base64Key = Convert.ToBase64String(keyBytes);

        return base64Key;
    }

    /// <summary>
    /// Helper to get the algorithm identifier string we chose.
    /// </summary>
    public static string GetAlgorithmIdentifier()
    {
        return SecurityAlgorithms.HmacSha256;
    }

    /// <summary>
    /// Helper to convert the stored Base64 key string back into a SymmetricSecurityKey
    /// needed for token generation/validation.
    /// </summary>
    /// <param name="base64Key">The Base64 encoded key string loaded from storage.</param>
    /// <returns>A SymmetricSecurityKey object.</returns>
    /// <exception cref="ArgumentNullException">Thrown if base64Key is null or whitespace.</exception>
    /// <exception cref="FormatException">Thrown if base64Key is not a valid Base64 string.</exception>
    public static SymmetricSecurityKey GetSecurityKeyFromBase64String(string base64Key)
    {
        if (string.IsNullOrWhiteSpace(base64Key))
        {
            throw new ArgumentNullException(nameof(base64Key), "Key string cannot be null or empty.");
        }

        // Convert the Base64 string back to bytes
        byte[] keyBytes = Convert.FromBase64String(base64Key);

        // Create the SymmetricSecurityKey object used by the JWT library
        var securityKey = new SymmetricSecurityKey(keyBytes);

        return securityKey;
    }
}